using System.Data;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using remoteWebMos.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;

namespace remoteWebMos
{
	public class MOSAPI
	{
		public static string GetExamDetailID(string exam_name, string project_name, string task_name)
		{
			DBAccess a = new DBAccess();
			string sql = @"select mos_exam_detail_id from mos_exam_detail d inner join mos_exam e on d.exam_id = e.exam_id
                              where e.exam_name like N'%" + exam_name + "%' and project like '%" + project_name + "%' and task like '%" + task_name + "%'";
			return a.ExecuteScalar(sql, CommandType.Text).ToString().Trim();
		}
		public static string JoinClass(string secret, string class_code)
		{
			try
			{
				DBAccess a = new DBAccess();
				a.ExecuteNonQuery("insert into class_user (uid_class, uid_user,join_date) values ((select class_id from class where class_code=@class_code), (select uid from mos_user where login_key=@secret ) ,getdate())", CommandType.Text,
					new string[] { "@class_code", "@secret" },
					new string[] { class_code, secret });
				return "Đã tham dự thành công lớp.";

				//Đã tham gia lớp của cô...
				//Mã lớp không tồn tại
				//ex.mess
			}
			catch (Exception ex)
			{
				return "Mã lớp không tồn tại, hoặc bạn đã tham gia lớp trước đó.";
			}
			return "";
		}

		public static string GetSeretKeyByUsername(string username)
		{
			DBAccess a = new DBAccess();
			return a.ExecuteScalar("select login_key from mos_user where login_key=@user", CommandType.Text,
				new string[] { "@user" },
				new string[] { username }).ToString().Trim();
		}
		public static DataTable LoadStudentListOfClass(string class_code)
		{
			DBAccess a = new DBAccess();
			return a.GetDataTable(@"select u.login_key, fullname, phone, email, exam_limit 
                        from class_user cu inner join  mos_user u on cu.uid_user = u.uid
                        where cu.uid_class=(select class_id from class where class_code=@class_code)
                        order by join_date desc
                        ", CommandType.Text,
						new string[] { "@class_code" },
						new string[] { class_code });
		}
		public static void DeleteClass(string class_code)
		{
			DBAccess a = new DBAccess();
			a.ExecuteNonQuery("DeleteClass", CommandType.StoredProcedure,
				new string[] { "@class_code" },
				new string[] { class_code });
		}

		public static string AddClass(string secret, string class_name)
		{
			try
			{
				DBAccess a = new DBAccess();
				a.ExecuteNonQuery("AddClass", CommandType.StoredProcedure,
					new string[] { "@secret", "@class_name" },
					new string[] { secret, class_name });
				return "ok";
			}
			catch
			{
				return "Tạo lớp không thành công, do đã đủ số lượng lớp tối đa.";
			}
		}
		public static DataTable LoadClass(string secret)
		{
			DBAccess a = new DBAccess();
			return a.GetDataTable("select * from class where class.uid_user=(select uid from mos_user where login_key=@secret) or class.uid_user_ta=(select uid from mos_user where login_key=@secret) order by date_created desc", CommandType.Text,
				new string[] { "@secret" }, new string[] { secret });
		}

		public static DataTable GetSubjectBySecret(string secret)
		{
			DBAccess a = new DBAccess();
			DataTable all_sub = a.GetDataTable("select exam_limit_category from mos_exam_limit_cat", CommandType.Text, new string[] { }, new string[] { });
			object exam_limit = a.ExecuteScalar("select exam_limit from mos_user where login_key=@key", CommandType.Text,
				new string[] { "@key" },
				new string[] { secret });
			if (exam_limit == null)
			{
				return all_sub;
			}
			else
			{
				if (exam_limit.ToString().Trim() == "")
				{
					return all_sub;
				}
				else
				{
					string[] s = exam_limit.ToString().Trim().Split(';');
					DataTable t = all_sub.Copy();
					t.Rows.Clear();
					t.AcceptChanges();
					foreach (string k in s)
					{
						t.Rows.Add(new string[] { k });
					}
					t.AcceptChanges();
					return t;
				}

			}
			return new DataTable();

		}
		public static string GetLicenseInfo(Int64 attempt_id)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select mos_attempt.uid_user,mos_user.fullname from mos_attempt inner join mos_user on mos_attempt.uid_user = mos_user.uid where mos_attempt.attempt_id = @id", CommandType.Text,
				new string[] { "@id" },
				new string[] { attempt_id.ToString() });
			if (t.Rows.Count > 0)
			{
				return t.Rows[0][1].ToString() + " " + t.Rows[0][0].ToString();
			}
			return "";
		}

		public static string GetLicenseInfo(string secret)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select mos_attempt.uid_user,mos_user.fullname from mos_attempt inner join mos_user on mos_attempt.uid_user = mos_user.uid where mos_user.login_key = @id", CommandType.Text,
				new string[] { "@id" },
				new string[] { secret });
			if (t.Rows.Count > 0)
			{
				return t.Rows[0][1].ToString() + " " + t.Rows[0][0].ToString();
			}
			return "";
		}


		public static DataTable GetExamList()
		{
			DBAccess a = new DBAccess();
			return a.GetDataTable("select * from mos_exam order by ordering asc", CommandType.Text);

		}

		public static int CheckDeviceID(string secret, string deviceID)
		{
			//return 1 là cho vào
			//return 0 là không cho vào
			DBAccess a = new DBAccess();
			object device = a.ExecuteScalar("select device_id from mos_user where login_key=@key", CommandType.Text,
				new string[] { "@key" },
				new string[] { secret });
			if (device is null)
			{
				a.ExecuteScalar("update mos_user set device_id=@device where login_key=@key", CommandType.Text,
					new string[] { "@device", "@key" },
					new string[] { deviceID, secret });
				return 1;
			}
			else //không phải NULL
			{
				if (device.ToString().Trim() == "") // có giá trị ""
				{
					a.ExecuteScalar("update mos_user set device_id=@device where login_key=@key", CommandType.Text,
					new string[] { "@device", "@key" },
					new string[] { deviceID, secret });
					return 1;
				}
				else //có giá trị khác ""
				{
					if (device.ToString().Trim().ToUpper() == deviceID.ToUpper().Trim() || device.ToString().Trim().ToUpper() == "GLOBAL")
						return 1;
					else
						return 0;
				}
			}

		}



		public static bool LoginSecretKey(string key)
		{
			//return
			//FirstLogin;SecondLogin;IncorrectKey
			DBAccess a = new DBAccess();
			DataTable kq = a.GetDataTable("select * from mos_user where login_key=@key and expired>=getdate()", CommandType.Text, new string[] { "@key" }, new string[] { key });
			if (kq.Rows.Count == 1)
			{
				if (kq.Rows[0]["login_user"] is null)
				{
					return true;
				}
				else
				{
					if (kq.Rows[0]["login_user"].ToString().Trim() == "")
						return true;
				}
				return true;
			}
			else
			{
				return false;
			}

		}

		public static bool LoginUserPass(string user, string pass)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select * from mos_user where login_user=@u and login_pass=@p and active='true' and expired>=getdate()", CommandType.Text,
				new string[] { "@u", "@p" }, new string[] { user, pass });
			if (t.Rows.Count > 0)
			{
				return true;
			}
			return false;
		}

		public static DataTable GetUserProfileBySecretkey(string secret)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select * from mos_user where login_key=@key", CommandType.Text,
				new string[] { "@key" }, new string[] { secret });
			return t;
		}

		public static string GetSecretByUser(string user)
		{
			DBAccess a = new DBAccess();
			object t = a.ExecuteScalar("select login_key from mos_user where login_user=@u", CommandType.Text,
				new string[] { "@u" }, new string[] { user });
			return t.ToString();
		}

		//Kiểm tra có phải saler còn hạn sử dụng không
		public static bool IsSalerAndExp(string secret)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select t0.active as saler_active, t1.active as acc_active, t1.expired from mos2019_sale t0 " +
				"inner join mos_user t1 on t0.uid_mos_user = t1.uid where login_key =@key", CommandType.Text,
				new string[] { "@key" }, new string[] { secret });
			if (t.Rows.Count > 0)
			{
				if (t.Rows[0]["saler_active"].ToString() == "True" && t.Rows[0]["acc_active"].ToString() == "True" && DateTime.Parse(t.Rows[0]["expired"].ToString()) >= DateTime.Now)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		//Kiểm tra tài khoản học sinh đã đăng ký remote và còn hạn sử dụng không
		public static bool IsRemotedAndExp(string secret)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select * from remote_machine_assign t0 inner join mos_user t1 on t0.uid_user = t1.uid where t1.login_key=@key", CommandType.Text,
				new string[] { "@key" }, new string[] { secret });
			if (t.Rows.Count > 0 && DateTime.Parse(t.Rows[0]["expired"].ToString()) >= DateTime.Now)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static DataTable GetSaleOrders(string secret)
		{
			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select * from mos2019_order t0 " +
				"inner join mos_user t1 on t0.uid_user_nguoicap = t1.uid where t1.login_key=@key", CommandType.Text,
				new string[] { "@key" }, new string[] { secret });
			return t;
		}

		public static string GetUserNameBySecret(string secret)
		{
			DBAccess a = new DBAccess();
			string t = a.ExecuteScalar("select fullname from mos_user t1 where t1.login_key=@key", CommandType.Text,
				new string[] { "@key" }, new string[] { secret }).ToString();
			return t;
		}

		public static string[] AddUser(string prefix, string expired, string attempt, string exam_limit)
		{
			try
			{
				DBAccess a = new DBAccess();
				string uid = a.ExecuteScalar("select MAX(uid) FROM mos_user", CommandType.Text).ToString();

				string newuid = (int.Parse(uid) + 1).ToString();
				// Insert user into mos_user table
				a.ExecuteNonQuery("SET IDENTITY_INSERT mos_user ON;INSERT INTO mos_user (uid,login_key,active, expired, attempt, exam_limit, device_id) VALUES (@newuid,@login_key,1, @expired, @attempt, @exam_limit,NULL)", CommandType.Text,
						new string[] { "@newuid", "@login_key", "@expired", "@attempt", "@exam_limit" },
						new string[] { newuid, Guid.NewGuid().ToString(), expired, attempt, exam_limit });


				string newLoginkey = prefix + newuid + new Random().Next(1000000).ToString("D6");

				DBAccess b = new DBAccess();
				// Update mos_user with the generated login key
				b.ExecuteNonQuery("UPDATE mos_user SET login_key = @login_key WHERE uid = @uid", CommandType.Text,
					new string[] { "@login_key", "@uid" },
					new string[] { newLoginkey, newuid });
				a.ExecuteNonQuery("SET IDENTITY_INSERT mos_user OFF", CommandType.Text);

				return new string[] { newuid, newLoginkey };
			}
			catch (Exception ex)
			{
				return null;
			}

		}

		public static string SendMail(string login_prefix, string email, string examlimits, string attempt, string expired)
		{
			string smtpServer = new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 10", CommandType.Text).ToString();
			string smtpUsername = new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 7", CommandType.Text).ToString();
			string smtpPassword = new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 8", CommandType.Text).ToString();

			// Cấu hình client SMTP
			SmtpClient smtpClient = new SmtpClient(smtpServer);
			smtpClient.Port = int.Parse(new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 11", CommandType.Text).ToString());
			smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
			smtpClient.EnableSsl = true;

			// Thông tin người gửi
			string fromEmail = new DBAccess().ExecuteScalar("select sender_email from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
					new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
			string fromName = new DBAccess().ExecuteScalar("select sender_name from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
					new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();

			string[] newUser = AddUser(login_prefix, expired, attempt, examlimits);
			string login_key = newUser[1];
			DBAccess a = new DBAccess();
			try
			{
				a.ExecuteNonQuery("Update mos_user set email=@Email where login_key = @loginkey", CommandType.Text,
					new string[] { "@Email", "@loginkey" }, new string[] { email, login_key });

				string subject = new DBAccess().ExecuteScalar("select email_sub from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
					new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
				string bodyTemplate = new DBAccess().ExecuteScalar("select email_body from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
					new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
				string mosBody = bodyTemplate.Replace("{fullname}", email).Replace("{login_key}", login_key).Replace("{expired}", expired).Replace("{subjects}", examlimits);

				string body = mosBody;

				MailMessage mail = new MailMessage();
				mail.From = new MailAddress(fromEmail, fromName);
				mail.IsBodyHtml = true;

				mail.To.Add(email);
				mail.Subject = subject;
				mail.Body = body;
				smtpClient.Send(mail);
				return newUser[0];

			}

			catch (Exception ex)
			{
				return null;
			}
		}
        public static bool ReSendMail(string login_key, string email, string examlimits, string attempt, string expired)
        {
            string smtpServer = new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 10", CommandType.Text).ToString();
            string smtpUsername = new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 7", CommandType.Text).ToString();
            string smtpPassword = new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 8", CommandType.Text).ToString();

            // Cấu hình client SMTP
            SmtpClient smtpClient = new SmtpClient(smtpServer);
            smtpClient.Port = int.Parse(new DBAccess().ExecuteScalar("select string_value from mos_setting where setting_id = 11", CommandType.Text).ToString());
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            smtpClient.EnableSsl = true;

            string login_prefix = new string(login_key.Where(char.IsLetter).ToArray());
            // Thông tin người gửi
            string fromEmail = new DBAccess().ExecuteScalar("select sender_email from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
                    new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
            string fromName = new DBAccess().ExecuteScalar("select sender_name from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
                    new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
            
            try
            {
                string subject = new DBAccess().ExecuteScalar("select email_sub from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
                    new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
                string bodyTemplate = new DBAccess().ExecuteScalar("select email_body from mos_prefix where loginkey_prefix = @loginkey_prefix", CommandType.Text,
                    new string[] { "@loginkey_prefix" }, new string[] { login_prefix }).ToString();
                string mosBody = bodyTemplate.Replace("{fullname}", email).Replace("{login_key}", login_key).Replace("{expired}", expired).Replace("{subjects}", examlimits);

                string body = mosBody;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail, fromName);
                mail.IsBodyHtml = true;

                mail.To.Add(email);
                mail.Subject = subject;
                mail.Body = body;
                smtpClient.Send(mail);
                return true;

            }

            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool AddOrders(string EmployID, string ngaycap, string emailKH, string CustomerID, string sotien, string kenhtt)
		{
			try
			{
                DBAccess a = new DBAccess();
                string oid = a.ExecuteScalar("select MAX(order_id) FROM mos2019_order", CommandType.Text).ToString();

                string newoid = (int.Parse(oid) + 1).ToString();int i = (int)DateTime.Now.DayOfWeek;
				string tien = sotien + "000";
                // Insert user into mos_user table
                a.ExecuteNonQuery("INSERT INTO mos2019_order VALUES (@newoid,@uid_user_nguoicap, @ngaycap, @email_khach,@uid_user_khach, @sotien,@kenhtt)", CommandType.Text,
                        new string[] { "@newoid", "@uid_user_nguoicap","@ngaycap", "@email_khach", "@uid_user_khach", "@sotien", "@kenhtt" },
                        new string[] { newoid,EmployID, ngaycap, emailKH, CustomerID, tien, kenhtt }); 
				return true;
            }
            catch(Exception ex)
			{
				return false;
			}
		}

		public static bool IsSection(string section)
		{
            if(IsDayofWeek(section) == true && IsTime(section) == true)
			{
				return true;
			}return false;
        }
		public static bool IsDayofWeek(string section)
		{
            int DOW = (int)DateTime.Now.DayOfWeek;
            List<List<int>> DaySection = new List<List<int>>
            {
                new List<int>{1,3,5},
                new List<int>{2,4,6},
                new List<int>{0}

            };
			if (section.Contains("246"))
			{
				if (DaySection[0].Contains(DOW))
				{
					return true;
				}
			}else if (section.Contains("357"))
			{
                if (DaySection[1].Contains(DOW))
                {
                    return true;
                }
			}
			else
			{
                if (DaySection[2].Contains(DOW))
                {
                    return true;
                }
            }
			return false;
        }

		public static bool IsTime(string section)
		{
			int h = DateTime.Now.Hour;
			int m = DateTime.Now.Minute;
            if (section.Contains("SANG"))
            {
                if (h>=3 && h<= 12)
                {
                    return true;
                }
            }
            else if (section.Contains("CHIEU"))
            {
                if (h >= 12 && h <= 18)
                {
                    return true;
                }
            }
            else
            {
                if (h <= 3 || h >= 18)
                {
                    return true;
                }
            }
            return false;
        }

		public static Remote GetInfRemote(string secret)
		{
			bool isexp = false;
			bool issection = false;

			DBAccess a = new DBAccess();
			DataTable t = a.GetDataTable("select t0.uid_user,t0.machine_uid,t0.note,t0.[from],t0.[to],t2.remote_desc,t2.username,t2.password from remote_machine_assign t0 " +
				"inner join mos_user t1 on t0.uid_user = t1.uid inner join remote_machine t2 on t0.machine_uid = t2.uid where t1.login_key = @key order by t0.uid", CommandType.Text,
				new string[] { "@key" }, new string[] { secret });

			if (DateTime.Now >= DateTime.Parse(t.Rows[0]["from"].ToString()) && DateTime.Now <= DateTime.Parse(t.Rows[0]["to"].ToString()))
			{
                isexp = true;
                if (IsSection(t.Rows[0]["note"].ToString()))
				{
					issection = true;
				}
			}
			return new Remote
			{
				uid = int.Parse(t.Rows[0]["uid_user"].ToString()),
				uidMachine = int.Parse(t.Rows[0]["machine_uid"].ToString()),
				section = t.Rows[0]["note"].ToString(),
				start = DateTime.Parse(t.Rows[0]["from"].ToString()),
				end = DateTime.Parse(t.Rows[0]["to"].ToString()),
				isExpired = isexp,
				isSection = issection,
				IdRemote = t.Rows[0]["remote_desc"].ToString(),
				userRemote = t.Rows[0]["username"].ToString(),
				passRemote = t.Rows[0]["password"].ToString()
			};
		}

		public static Models.SalerModel GetOrder(int idorder,string secret)
		{
            DBAccess a = new DBAccess();
            DataTable t = a.GetDataTable("select order_id, t1.email, exam_limit,attempt,ngaycap,expired,sotien,kenhtt, login_key, t1.uid from mos2019_order t0 " +
				"inner join mos_user t1 on t0.uid_user_khach = t1.uid where t0.order_id = @oid", CommandType.Text,
                new string[] { "@oid" }, new string[] { idorder.ToString() });

			SalerModel salemodel = new SalerModel().KhoiTao(secret);
			
            SaleOrder order = new SaleOrder()
			{
				OrderId = idorder,
				EmailKH = t.Rows[0]["email"].ToString(),
				NgayCap = DateTime.Parse(t.Rows[0]["ngaycap"].ToString()),
				Sotien = int.Parse(t.Rows[0]["sotien"].ToString().Substring(0, t.Rows[0]["sotien"].ToString().Length-3)),
				KenhTT = t.Rows[0]["kenhtt"].ToString(),
				EmployID = salemodel.saleOrder.EmployID
            };
			Models.AccountModel accountModel = new Models.AccountModel()
			{
				Id= int.Parse(t.Rows[0]["uid"].ToString()),
				Attempt = int.Parse(t.Rows[0]["attempt"].ToString()),
				ExamLimits = t.Rows[0]["exam_limit"].ToString(),
				Expired = DateTime.Parse(t.Rows[0]["expired"].ToString()),
				login_prefix = new string((t.Rows[0]["login_key"].ToString()).Where(char.IsLetter).ToArray()),
				login_key = t.Rows[0]["login_key"].ToString()
            };

			salemodel.saleOrder= order;
			salemodel.account = accountModel;
			foreach(var option in salemodel.CheckboxOptions)
			{
				if (accountModel.ExamLimits.Contains(option.value))
				{
					option.IsChecked= true;
				}
			}
			return salemodel;
			
        }

		public static bool UpdateEmailUser(string uid,string orderid,string email)
		{
			try
			{
				DBAccess a = new DBAccess();
				a.ExecuteNonQuery("update mos_user set email = @email where uid =@uid", CommandType.Text,
					new string[] { "@email", "@uid" }, new string[] { email, uid });
                a.ExecuteNonQuery("update mos2019_order set email_khach = @email where order_id =@orderid", CommandType.Text,
                    new string[] { "@email", "@orderid" }, new string[] { email, orderid });
                return true;
			}catch(Exception ex)
			{
				return false;
			}
		}


		private const int KeySize = 256;
		private const int BlockSize = 128;

		public static string Encrypt(string plainText, byte[] keyBytes, string iv)
		{
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

			using (Aes aes = Aes.Create())
			{
				aes.KeySize = KeySize;
				aes.BlockSize = BlockSize;
				aes.Key = keyBytes;
				aes.IV = Encoding.UTF8.GetBytes(iv);
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
					{
						cs.Write(plainBytes, 0, plainBytes.Length);
						cs.FlushFinalBlock();

						byte[] cipherBytes = ms.ToArray();
						return Convert.ToBase64String(cipherBytes);
					}
				}
			}
		}

		public static string Decrypt(string cipherText, byte[] keyBytes, string iv)
		{
			byte[] cipherBytes = Convert.FromBase64String(cipherText);

			using (Aes aes = Aes.Create())
			{
				aes.KeySize = KeySize;
				aes.BlockSize = BlockSize;
				aes.Key = keyBytes;
				aes.IV = Encoding.UTF8.GetBytes(iv);
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;

				using (MemoryStream ms = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(cipherBytes, 0, cipherBytes.Length);
						cs.FlushFinalBlock();

						byte[] plainBytes = ms.ToArray();
						return Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
					}
				}
			}
		}

		public static string Decrypt(string encrypt_Text)
		{
			string encryptedText = encrypt_Text;
			byte[] keyBytes = new byte[32];
			for (int i = 0; i < keyBytes.Length; i++)
			{
				keyBytes[i] = 0xFF;
			}
			string iv = "my-secret-iv-456";

			string decryptedText = remoteWebMos.MOSAPI.Decrypt(encrypt_Text, keyBytes, iv);
			return decryptedText;
		}
	}
}
