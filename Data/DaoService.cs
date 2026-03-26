using System.Data;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.SqlClient;

namespace FinTrack.Data;

public class DaoService
{
    private readonly string _stringConexao;

    public DaoService(IConfiguration configuration)
    {
        _stringConexao = configuration.GetConnectionString("FinTrack");
    }

    private void ExecutarProcedure(string procedure, Dictionary<string, object> parametros)
    {
        List<SqlError> erros = null;
        
        SqlConnection conn = new SqlConnection(_stringConexao);

        conn.Open();

        SqlCommand cmmd = NovoCmmd(procedure, conn);
        
        AdicionarParametros(cmmd, parametros);
        
        conn.FireInfoMessageEventOnUserErrors = true;

        conn.InfoMessage += new SqlInfoMessageEventHandler((object sender, SqlInfoMessageEventArgs e) =>
        {
            if (erros == null)
            {
                erros = new List<SqlError>();
            }

            foreach (SqlError error in e.Errors)
            {
                erros.Add(error);
            }
        });
        
        cmmd.ExecuteNonQuery(); 
        
        if (erros != null)
        {
            throw new ErroExecucaoException(erros);
        }

        cmmd.Dispose(); 

        conn.Close(); 
        conn.Dispose(); 
    }

        public List<T> ExecutarProcedureList<T>(string procedure, Dictionary<string, object> parametros)
    {
        List<T> list = null; 

        List<SqlError> erros = null;

        SqlConnection conn = new SqlConnection(_stringConexao); 

        conn.Open();  

        SqlCommand cmmd = NovoCmmd(procedure, conn); 

        AdicionarParametros(cmmd, parametros); 

        conn.FireInfoMessageEventOnUserErrors = true;
        
        conn.InfoMessage += new SqlInfoMessageEventHandler((object sender, SqlInfoMessageEventArgs e) =>
        {
            if (erros == null)
            {
                erros = new List<SqlError>();
            }

            foreach (SqlError error in e.Errors)
            {
                //adiciona um erro na lista
                erros.Add(error);
            }
        });

        SqlDataReader dr = cmmd.ExecuteReader(); 

        if (erros != null)
        {
            throw new ErroExecucaoException(erros);
        }

        list = CriarLista<T>(dr); 

        
        dr.Close(); 
        dr.Dispose();
        
        cmmd.Dispose(); 

       
        conn.Close();
        conn.Dispose();

        return list;
    }
    

    private List<T> CriarLista<T>(SqlDataReader dr)
    {
        List<T> list = null;

        
        if (dr.HasRows) 
        {
            list = new List<T>();
            
            while (dr.Read()) 
            {
                var item = Activator.CreateInstance<T>();
                
                foreach (var property in typeof(T).GetProperties()) 
                {
                    string nomecoluna;
                    
                    if (property.GetCustomAttribute<ColumnAttribute>() != null) 
                    {
                        nomecoluna = property.GetCustomAttribute<ColumnAttribute>().Name;
                    }
                    else
                    {
                        nomecoluna = property.Name; 
                    }
                    
                    int i = GetColumnOrginal(dr, nomecoluna);
                    
                    if (i < 0) continue;
                    
                    if (dr[nomecoluna] ==  DBNull.Value) continue; 

                    if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(item, Enum.Parse(property.PropertyType, dr[nomecoluna].ToString()));
                    }
                    else
                    {
                        Type convertTo = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        
                        property.SetValue(item, Convert.ChangeType(dr[nomecoluna],convertTo));
                    }
                }
                list.Add(item);
            }
        }
        return list;
    }

    private int GetColumnOrginal(SqlDataReader dr, string columName)
    {
       
        int ordinal = -1;
        
        for (int i = 0; i < dr.FieldCount; i++) 
        {
            if(string.Equals(dr.GetName(i), columName, StringComparison.OrdinalIgnoreCase))
            {
                ordinal = i; 
                break;
            }
        }

        return ordinal;
        
    }

    

    private SqlCommand NovoCmmd(string procedure, SqlConnection conn)
    {
        return new SqlCommand(procedure, conn)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60
        };
    }

    private void AdicionarParametros(SqlCommand cmmd, Dictionary<string, object> parametros)
    {
        if (parametros != null)
        {
            foreach (var item in parametros)
            {
                cmmd.Parameters.AddWithValue(item.Key, item.Value);
            }   
        }
    }
}