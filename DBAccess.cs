using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;

namespace remoteWebMos
{
	public class DBAccess
	{
		#region Declarations
		protected SqlConnection sc = null;
		#endregion

		#region Constructors
		/// <summary>
		/// Hàm khởi dựng
		/// thực hiện mở Connection
		/// </summary>
		

		public DBAccess()
		{
			if (sc == null)
			{
				sc = new SqlConnection();
				
				string strcon = "secret";


                string encryptedText = sc.ConnectionString;
				byte[] keyBytes = new byte[32];
				for (int i = 0; i < keyBytes.Length; i++)
				{
					keyBytes[i] = 0xFF;
				}
				string iv = "my-secret-iv-456";

				string decryptedText = remoteWebMos.MOSAPI.Decrypt(strcon, keyBytes, iv);
				if (decryptedText != null)
				{
					sc.ConnectionString = decryptedText;
				}



			}
		}

		public DBAccess(string ConStrName)
		{
			if (sc == null)
			{
				sc = new SqlConnection();
				//sc.ConnectionString = WebConfigurationManager.ConnectionStrings["ConStr"].ConnectionString();
				//sc.ConnectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;
				//"server=thinhdt-ultra;database=LibUEH;uid=sa;pwd=davn7710";
				//sc.ConnectionString = "server=.;database=LibUEH;uid=sa;pwd=123456";
				sc.ConnectionString = System.Configuration.ConfigurationManager.AppSettings[ConStrName].ToString();
			}
		}


		public void Open()
		{
			if (sc.State != ConnectionState.Open)
				sc.Open();
		}
		public void Close()
		{
			if (sc.State != ConnectionState.Closed)
				sc.Close();

		}

		#endregion

		#region Methods
		//---------
		/// <summary>
		/// Hàm lấy dữ liệu từ database trả về 1 DataSet
		/// Không chứa Parameter 
		/// </summary>
		/// <param name="Query">Chuỗi SQL</param>
		/// <param name="command_type">Loại Command như Text, Store Procedure</param>
		/// <returns>DataSet</returns>
		public DataSet GetDataSet(string Query, CommandType command_type)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				DataSet dataset = new DataSet();
				SqlCommand command = new SqlCommand(Query, sc);
				command.CommandType = command_type;
				SqlDataAdapter sda = new SqlDataAdapter(command);
				sda.Fill(dataset);
				return dataset;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return null;

		}
		/// <summary>
		/// Hàm lấy dữ liệu từ database trả về 1 DataSet
		/// Có chứa Parameter
		/// </summary>
		/// <param name="Query">Chuỗi SQL</param>
		/// <param name="command_type">Loại Command như Text, Store Procedure</param>
		/// <param name="parameter_name">Mảng kiểu chuỗi chứa tên các tham số</param>
		/// <param name="value">Mảng kiểu chuỗi chứa giá trị các tham số</param>
		/// <returns>DataSet</returns>
		public DataSet GetDataSet(string Query, CommandType command_type, string[] parameter_name, string[] value)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				DataSet m_dataset = new DataSet();
				SqlCommand command = new SqlCommand();
				command.CommandText = Query;
				command.CommandType = command_type;
				command.Connection = sc;
				for (int i = 0; i < parameter_name.Length; i++)
				{
					SqlParameter m_parameter = new SqlParameter();
					m_parameter.ParameterName = parameter_name[i];
					m_parameter.Value = value[i];
					command.Parameters.Add(m_parameter);
				}
				SqlDataAdapter sda = new SqlDataAdapter(command);
				sda.Fill(m_dataset);
				return m_dataset;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return null;

		}
		/// <summary>
		/// Hàm lấy dữ liệu từ database trả về 1 DataTable
		/// Không chứa Parameter
		/// </summary>
		/// <param name="Query">Chuỗi SQL</param>
		/// <param name="command_type">Loại Command như Text, Store Procedure</param>
		/// <returns>DataTable</returns>
		public DataTable GetDataTable(string Query, CommandType command_type)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				DataTable m_datatable = new DataTable();
				SqlCommand command = new SqlCommand(Query, sc);
				command.CommandType = command_type;
				SqlDataAdapter sda = new SqlDataAdapter(command);
				sda.Fill(m_datatable);
				return m_datatable;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return null;

		}
		/// <summary>
		/// Hàm lấy dữ liệu từ database trả về 1 DataTable
		/// Có chứa Parameter
		/// </summary>
		/// <param name="Query">Chuỗi SQL</param>
		/// <param name="command_type">Loại Command như Text, Store Procedure</param>
		/// <param name="parameter_name">Mảng kiểu chuỗi chứa tên các tham số</param>
		/// <param name="value">Mảng kiểu chuỗi chứa giá trị các tham số</param>
		/// <returns>DataTable</returns>
		public DataTable GetDataTable(string Query, CommandType command_type, string[] parameter_name, string[] value)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				DataTable m_datatable = new DataTable();
				SqlCommand command = new SqlCommand();
				command.CommandText = Query;
				command.CommandType = command_type;
				command.Connection = sc;
				for (int i = 0; i < parameter_name.Length; i++)
				{
					SqlParameter m_parameter = new SqlParameter();
					m_parameter.ParameterName = parameter_name[i];
					m_parameter.Value = value[i];
					command.Parameters.Add(m_parameter);
				}
				SqlDataAdapter sda = new SqlDataAdapter(command);
				sda.Fill(m_datatable);
				return m_datatable;
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return null;

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Query"></param>
		/// <param name="command_type"></param>
		/// <returns></returns>
		public int ExecuteNonQuery(string Query, CommandType command_type)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand command = new SqlCommand();
				command.CommandType = command_type;
				command.CommandText = Query;
				command.Connection = sc;
				return command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return 0;
		}
		public int ExecuteNonQuery(string Query, CommandType command_type, string[] parameter_name, string[] value)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand command = new SqlCommand();
				command.CommandText = Query;
				command.CommandType = command_type;
				command.Connection = sc;
				for (int i = 0; i < parameter_name.Length; i++)
				{
					SqlParameter m_parameter = new SqlParameter();
					m_parameter.ParameterName = parameter_name[i];
					m_parameter.Value = value[i];
					command.Parameters.Add(m_parameter);
				}
				return command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return 0;
		}

		public int ExecuteNonQueryManualOpen(string Query, CommandType command_type, string[] parameter_name, string[] value)
		{
			try
			{
				//if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand command = new SqlCommand();
				command.CommandText = Query;
				command.CommandType = command_type;
				command.Connection = sc;
				for (int i = 0; i < parameter_name.Length; i++)
				{
					SqlParameter m_parameter = new SqlParameter();
					m_parameter.ParameterName = parameter_name[i];
					m_parameter.Value = value[i];
					command.Parameters.Add(m_parameter);
				}
				return command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				//sc.Close();
			}
			return 0;
		}

		public object ExecuteScalar(string Query, CommandType command_type)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand command = new SqlCommand(Query, sc);
				command.CommandType = command_type;
				return command.ExecuteScalar();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return null;
		}

		public object ExecuteScalar(string Query, CommandType command_type, string[] parameter_name, string[] value)
		{
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand command = new SqlCommand();
				command.CommandText = Query;
				command.CommandType = command_type;
				command.Connection = sc;
				for (int i = 0; i < parameter_name.Length; i++)
				{
					SqlParameter m_parameter = new SqlParameter();
					m_parameter.ParameterName = parameter_name[i];
					m_parameter.Value = value[i];
					command.Parameters.Add(m_parameter);
				}
				return command.ExecuteScalar();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				sc.Close();
			}
			return null;
		}


		public object ExecuteScalarManualOpenConn(string Query, CommandType command_type, string[] parameter_name, string[] value)
		{
			try
			{
				//if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand command = new SqlCommand();
				command.CommandText = Query;
				command.CommandType = command_type;
				command.Connection = sc;
				for (int i = 0; i < parameter_name.Length; i++)
				{
					SqlParameter m_parameter = new SqlParameter();
					m_parameter.ParameterName = parameter_name[i];
					m_parameter.Value = value[i];
					command.Parameters.Add(m_parameter);
				}
				return command.ExecuteScalar();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				//sc.Close();
			}
			return null;
		}

		/// <summary>
		/// execute a query SQL statement and return a OleDbDataReader object    
		/// </summary>
		/// <param name="Query">SQL Query</param>
		/// <param name="command_type">Command type. Example: Text/Table/Store Procedure</param>
		/// <returns>SqlDataReader</returns>
		public SqlDataReader GetDataReader(string Query, CommandType command_type)
		{
			SqlDataReader dr = null;

			//query data
			try
			{
				if (sc.State != ConnectionState.Open) sc.Open();
				SqlCommand cmd = new SqlCommand(Query, sc);
				cmd.CommandType = command_type;
				dr = cmd.ExecuteReader();
				return dr;
			}
			catch (Exception e)
			{
				throw e;
			}
			finally
			{
				sc.Close();
			}
		}

		/// <summary>
		/// Execute SQL script file.    
		/// </summary>
		/// <param name="sFilePath: Duong dan chua file .sql"></param>    
		public int ExecuteScriptFile(string sFilePath)
		{
			int intReturn = -1;
			if (System.IO.File.Exists(sFilePath))
			{
				System.IO.StreamReader sr = System.IO.File.OpenText(sFilePath);
				string script = sr.ReadToEnd();
				sr.Close();
				intReturn = ExecuteScript(script, true);
			}
			return intReturn;
		}

		/// <summary>
		/// Chay nhieu cau sql mot luc.
		/// Run script sql.
		/// </summary>
		/// <param name="sScriptSQL"></param>
		/// <param name="bUseTransactions"></param>
		public int ExecuteScript(string sScriptSQL, bool bUseTransactions)
		{
			int nReturn = 0;
			string sDelimiter = "GO" + "\r\n";
			string[] arrSQL = SplitByString(sScriptSQL, sDelimiter);

			if (bUseTransactions)
			{
				//Get sql command
				SqlCommand objSqlCommand = null;
				SqlTransaction sqlTrans = null;
				try
				{
					if (sc.State != ConnectionState.Open) sc.Open();
					sqlTrans = sc.BeginTransaction();

					objSqlCommand = new SqlCommand("s", sc);
					objSqlCommand.Transaction = sqlTrans;

					foreach (string sSQL in arrSQL)
					{
						if (sSQL != "")
						{
							objSqlCommand.CommandText = sSQL;
							objSqlCommand.ExecuteNonQuery();
						}
					}
					sqlTrans.Commit();
					nReturn = 1;
				}
				catch (SqlException)
				{
					if (sqlTrans != null)
						sqlTrans.Rollback();
					nReturn = -1;
				}
				finally
				{
					if (sc != null)
						sc.Close();
				}
			}
			return nReturn;
		}

		/// <summary>
		/// Dat them de sDelimiter string by string
		/// Cat chuoi thanh nhieu manh bang chuoi cung cap.
		/// </summary>
		/// <param name="sSplit: Chuoi can phan manh"></param>
		/// <param name="sDelimiter: Chuoi gia tri phan manh"></param>    
		public string[] SplitByString(string sSplit, string sDelimiter)
		{
			int offset = 0;
			int index = 0;
			int[] offsets = new int[sSplit.Length + 1];

			while (index < sSplit.Length)
			{
				int indexOf = sSplit.IndexOf(sDelimiter, index);
				if (indexOf != -1)
				{
					offsets[offset++] = indexOf;
					index = (indexOf + sDelimiter.Length);
				}
				else
				{
					index = sSplit.Length;
				}
			}

			string[] final = new string[offset + 1];
			if (offset == 0)
			{
				final[0] = sSplit;
			}
			else
			{
				offset--;
				final[0] = sSplit.Substring(0, offsets[0]);
				for (int i = 0; i < offset; i++)
				{
					final[i + 1] = sSplit.Substring(offsets[i] + sDelimiter.Length, offsets[i + 1] - offsets[i] - sDelimiter.Length);
				}
				final[offset + 1] = sSplit.Substring(offsets[offset] + sDelimiter.Length);
			}
			return final;
		}

		#endregion
	}
	public class appSetting
	{
		public string xml { get; set; }
	}
}
