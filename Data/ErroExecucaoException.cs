using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace FinTrack.Data;

public class ErroExecucaoException : Exception
{
    public List<ErroExecucao> Erros;
    
    public ErroExecucaoException(List<SqlError> erros)
    {
        Erros = new List<ErroExecucao>();
        foreach (SqlError item in erros)
        {
            if(item.Number == 3600) return;
            if (item.Number == 50001)
            {

                ErroExecucao erro = JsonSerializer.Deserialize<ErroExecucao>(item.Message);
                
                Erros.Add(erro);
            }
            else
            {
                throw new Exception(item.Message);
            }
            
        }
    }
    
    public class ErroExecucao
    {
        public string NomeInput { get; set; }
        public string Mensagem { get; set; }
    }
}