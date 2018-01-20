using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptureX
{
    class ImageData
    {
        public SQLiteConnection OpenConnection()
        {
            string connstr = "Data Source=CaptureX.db;Pooling=true;FailIfMissing=false";
            SQLiteConnection connection = new SQLiteConnection(connstr);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Add(Image img)
        {
            using (IDbConnection conn = OpenConnection())
            {
                //conn.BeginTransaction();
                string ID = Guid.NewGuid().ToString();
                conn.Execute("INSERT INTO img_info(id,create_at,paste_at,export_at) VALUES(@ID,DATETIME(),NULL,NULL)", new { ID=ID});
                return conn.Execute("INSERT INTO images(id,image) VALUES(@ID,@image)", new { ID = ID, image = BitmapToBytes(new Bitmap(img)) });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            using (IDbConnection conn = OpenConnection())
            {
                return (Int32)conn.Query("SELECT COUNT(1) AS CNT FROM img_info WHERE paste_at IS NULL").First().CNT;
            }
        }

        public int AllCount()
        {
            using (IDbConnection conn = OpenConnection())
            {
                return (Int32)conn.Query("SELECT COUNT(1) AS CNT FROM img_info ").First().CNT;
            }
        }

        public Bitmap GetFristImage()
        {
            string query = @"
                            SELECT A.ID,B.image 
                            FROM img_info AS A
                            INNER JOIN images AS B
                            ON A.id = B.Id
                            WHERE paste_at IS NULL
                            ORDER BY create_at ASC
                            LIMIT 1
                        ";
            using (IDbConnection conn = OpenConnection())
            {
                var imageData = conn.Query(query);
                if (imageData.Count()==0) return null;

                conn.Execute("UPDATE img_info SET paste_at=DATETIME() WHERE id=@ID", new { ID = imageData.First().id });
                return BytesToBitmap(imageData.First().image);
            }            
        }

        public Bitmap GetExportImage()
        {
            string query = @"
                            SELECT A.ID,B.image 
                            FROM img_info AS A
                            INNER JOIN images AS B
                            ON A.id = B.Id
                            WHERE export_at IS NULL
                            ORDER BY create_at ASC
                            LIMIT 1
                        ";
            using (IDbConnection conn = OpenConnection())
            {
                var imageData = conn.Query(query);
                if (imageData.Count() == 0) return null;

                conn.Execute("UPDATE img_info SET export_at=DATETIME() WHERE id=@ID", new { ID = imageData.First().id });
                return BytesToBitmap(imageData.First().image);
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <returns></returns>
        public int Clear()
        {
            using (IDbConnection conn = OpenConnection())
            {
                conn.Execute("DELETE FROM images");
                return conn.Execute("DELETE FROM img_info"); 
            } 
        }

        /// <summary>
        /// byte[] 转图片 
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public Bitmap BytesToBitmap(byte[] Bytes)  
        {  
            MemoryStream stream = null;  
            try  
            {  
                stream = new MemoryStream(Bytes);  
                return new Bitmap((Image)new Bitmap(stream));  
            }  
            catch (ArgumentNullException ex)  
            {  
                throw ex;  
            }  
            catch (ArgumentException ex)  
            {  
                throw ex;  
            }  
            finally  
            {  
                stream.Close();  
            }  
        }  

        /// <summary>
        /// 图片转byte[]
        /// </summary>
        /// <param name="Bitmap"></param>
        /// <returns></returns>
        public byte[] BitmapToBytes(Bitmap Bitmap)  
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                Bitmap.Save(ms, ImageFormat.Png);
                byte[] byteImage = new Byte[ms.Length];
                byteImage = ms.ToArray();
                return byteImage;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            } 
        }  
    }
}
