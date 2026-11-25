using Microsoft.Data.SqlClient;
using StudentManagement.Helpers;
using StudentManagement.Models;
using System.Data;

namespace StudentManagement.Repositories
{
    public class FileRepository
    {
        private readonly SqlHelper _sqlHelper;

        public FileRepository(SqlHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        // INSERT
        public async Task< int> InsertFile(FileRecord file)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter(
                    "@OriginalFileName",
                    file.OriginalFileName),

                new SqlParameter(
                    "@StoredFileName",
                    file.StoredFileName),

                new SqlParameter(
                    "@FilePath",
                    file.FilePath),

                new SqlParameter(
                    "@FileSize",
                    file.FileSize),

                new SqlParameter(
                    "@ContentType",
                    (object?)file.ContentType ??
                    DBNull.Value),

                new SqlParameter(
                    "@CreatedDate",
                    file.CreatedDate)
            };

            object result =
                _sqlHelper.ExecuteNonQuery(
                    "sp_InsertFile",
                    parameters);

            return Convert.ToInt32(result);
        }


        // GET BY ID
        public async Task<FileRecord?> GetFileById(int id)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Id", id)
            };

            DataTable dt =
                _sqlHelper.GetData(
                    "sp_GetFileById",
                    parameters);

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = dt.Rows[0];

            return new FileRecord
            {
                Id =
                    Convert.ToInt32(
                        row["Id"]),

                OriginalFileName =
                    row["OriginalFileName"]
                        .ToString()!,

                StoredFileName =
                    row["StoredFileName"]
                        .ToString()!,

                FilePath =
                    row["FilePath"]
                        .ToString()!,

                FileSize =
                    Convert.ToInt64(
                        row["FileSize"]),

                ContentType =
                    row["ContentType"] ==
                    DBNull.Value
                        ? null
                        : row["ContentType"]
                            .ToString(),

                CreatedDate =
                    Convert.ToDateTime(
                        row["CreatedDate"])
            };
        }


        // DELETE
        public async Task<int> DeleteFile(int id)
        {
            try
            {
                SqlParameter[] parameters =
           {
                new SqlParameter(
                    "@Id",
                    id)
            };

                return _sqlHelper.ExecuteNonQuery(
                    "sp_DeleteFile",
                    parameters);
            
           
        }
            catch(Exception ex)
                {
                return 0;
            }

        }


        // GET ALL FILES
        public async Task<List<FileRecord>> GetAllFiles()
        {
            DataTable dt =
                _sqlHelper.GetData(
                    "sp_GetAllFiles");

            List<FileRecord> files =
                new List<FileRecord>();

            foreach (DataRow row in dt.Rows)
            {
                files.Add(
                    new FileRecord
                    {
                        Id =
                            Convert.ToInt32(
                                row["Id"]),

                        OriginalFileName =
                            row["OriginalFileName"]
                                .ToString()!,

                        StoredFileName =
                            row["StoredFileName"]
                                .ToString()!,

                        FilePath =
                            row["FilePath"]
                                .ToString()!,

                        FileSize =
                            Convert.ToInt64(
                                row["FileSize"]),

                        ContentType =
                            row["ContentType"] ==
                            DBNull.Value
                                ? null
                                : row["ContentType"]
                                    .ToString(),

                        CreatedDate =
                            Convert.ToDateTime(
                                row["CreatedDate"])
                    });
            }

            return files;
        }
    }
}
