using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace StudentManagement.Helpers
{
  public class SqlHelper
    {
        private readonly string _connectionString;

        public SqlHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Connection");
        }

        public DataTable GetData(string procedureName, params SqlParameter[] parameters)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(procedureName, con);

            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using SqlDataAdapter da = new SqlDataAdapter(cmd);

            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public int ExecuteNonQuery(string procedureName, params SqlParameter[] parameters)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_connectionString);
                using SqlCommand cmd = new SqlCommand(procedureName, con);

                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();

                return cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        public object ExecuteScalar(string procedureName, params SqlParameter[] parameters)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(procedureName, con);

            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            con.Open();

            return cmd.ExecuteScalar();
        }
    }
}