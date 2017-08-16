using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DawnOfWar_server.DataBase
{
    class DbConnection
    {
        private static SqlConnection GetConnection()
        {
            var connection = "Data Source=" + Model.server + ";Initial Catalog=ProjectAuth_DB;Integrated security=False;User Id=" + Model.login + ";Password = " + Model.pass + ";";
            SqlConnection conn = new SqlConnection(connection);
            try
            {
                conn.Open();
            }
            catch
            {
                conn = null;
            }
            return conn;
        }

        //internal static void RegistrateClient(string guidUSB, string guidClient, Certificate certificate)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return;
        //    string comm = @"EXEC [dbo].[InsertCertificate] @publicKey, @type, @start, @stop, @date, @guidClient ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("publicKey", certificate.publicKey);
        //    cmd.Parameters.AddWithValue("type", 2);
        //    cmd.Parameters.AddWithValue("start", certificate.dateStart);
        //    cmd.Parameters.AddWithValue("stop", certificate.dateStop);
        //    cmd.Parameters.AddWithValue("date", certificate.dateCreating);
        //    cmd.Parameters.AddWithValue("guidClient", guidClient);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    sdr.Read();
        //    int id = sdr.GetInt32(0);
        //    sdr.Close();
        //    sdr = null;
        //    cmd.Cancel();
        //    cmd = null;
        //    comm = @"EXEC [dbo].[RegistrateClient] @guidUsb,@guidClient,@idCert ";
        //    cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guidUsb", guidUSB);
        //    cmd.Parameters.AddWithValue("guidClient", guidClient);
        //    cmd.Parameters.AddWithValue("idCert", id);
        //    cmd.ExecuteNonQuery();
        //    cmd.Cancel();
        //    cmd = null;
        //    SetCertificateStatus(guidClient, "active");
        //}

        private void SetCertStatus(string guid, string statusNum)
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"INSERT INTO CertificateStatus VALUES((Select Certificate.id from Certificate where Certificate.guid = @guidCert),@status,GETDATE())";

            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guidCert", guid);
            cmd.Parameters.AddWithValue("status", statusNum);
            cmd.ExecuteNonQuery();
        }

        //internal static void GetOnlineClients(ref List<objClient> list)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //    {
        //        list.Clear();
        //        return;
        //    }
        //    string comm = "SELECT        [State].[Date], Client.[Address], Client.[Guid]" +
        //                " FROM(SELECT        ClientState_1.ClientId, ClientState_1.[Date], ClientState_1.StateId" +
        //                  " FROM(SELECT        MAX(date) AS DateT, clientId" +
        //                  "                          FROM            ClientState" +
        //                  "                          GROUP BY clientId) AS Filter LEFT OUTER JOIN" +
        //                  "                          ClientState AS ClientState_1 ON Filter.clientId = ClientState_1.ClientId AND Filter.DateT = ClientState_1.[date]) AS[State] INNER JOIN" +
        //                " Client ON[State].ClientId = Client.Id" +
        //                 "   WHERE([State].StateId = 2)";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    list.Clear();
        //    while (sdr.Read())
        //    {
        //        list.Add(new objClient(sdr.GetValue(2).ToString(), sdr.GetValue(1).ToString(), sdr.GetDateTime(0)));
        //    }
        //}

        internal static void ClientAlive(string guidClient)
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"Update [dbo].[ClientState] SET [date]=@date where id = (select TOP(1) id from ClientState where clientid = (select id from client where guid = @guid) Order by date desc)";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidClient);
            cmd.Parameters.AddWithValue("date", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        //internal static string GetLoginClientByGuid(object guidClient)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"Select TOP(1) [login] from [User] inner join Client on client.userId=[user].id where client.guid = @guid";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guidClient);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    if (sdr.Read())
        //    {
        //        return sdr.GetString(0);
        //    }
        //    else
        //    {
        //        AddLogs("Не существующий guid", "Попытка выполнения действия клиента с несуществующим guid", TypeLog.violation);
        //        return "'не найден'";
        //    }
        //}

        //internal static string GetNameServerByGuid(string guid)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"Select TOP(1) name from Server where Server.guid = @guid";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guid);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    if (sdr.Read())
        //    {
        //        return sdr.GetString(0);
        //    }
        //    else
        //    {
        //        AddLogs("Не существующий guid", "Попытка выполнения действия сервера с несуществующим guid", TypeLog.BigViolation);
        //        return "'не найден'";
        //    }
        //}

        internal static bool CheckStateClient(string guidClient, string ip)
        {
            var conn = GetConnection();
            if (conn == null)
                return false;
            string comm = @"Select TOP(1) ClientState.stateid, client.address, ClientState.date from ClientState inner join Client on clientState.clientid=client.id where clientid = (select id from client where guid = @guid) Order by date desc";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidClient);
            SqlDataReader sdr = cmd.ExecuteReader();
            if (sdr.Read())
            {
                if (sdr.GetInt32(0) == 2 && sdr.GetValue(1).ToString() == ip && sdr.GetDateTime(2).AddMinutes(5) > DateTime.Now)
                    return true;
            }
            return false;
        }

        internal static void SetPartKey(string guidUSB, string partKey)
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"Update USB SET partKey=@key where guid = @guid";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidUSB);
            cmd.Parameters.AddWithValue("key", partKey);
            cmd.ExecuteNonQuery();
        }

        internal static string[,] GetPortsForClient(string guidClient)
        {
            var conn = GetConnection();
            if (conn == null)
                return null;
            string comm = @"SELECT (SELECT [Server].[address] from [Server] WHERE id =[profile].serverId ), (SELECT Program.[port]+':'+Program.protocol from Program WHERE id = [profile].programId) FROM [Profile] right join (SELECT profileId FROM ClientProfile WHERE ClientProfile.clientId=(Select id from Client where [guid] = @guid)) as usrProf on [profile].id=usrProf.profileId";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidClient);
            var reader = cmd.ExecuteReader();

            int count = 0;
            while (reader.Read())
                count++;
            var data = new string[count, 2];
            reader.Close();
            reader = cmd.ExecuteReader();
            count = 0;
            while (reader.Read())
            {
                data[count, 0] = reader.GetString(0);
                data[count, 1] = reader.GetString(1);
                count++;
            }
            return data;
        }

        internal static string GetGuidClientOfUsb(string guidUsb)
        {
            var conn = GetConnection();
            if (conn == null)
                return null;
            string comm = @"Select Client.guid from usb left join client on usb.id=client.usbid where usb.guid=@guid";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidUsb);
            SqlDataReader sdr = cmd.ExecuteReader();
            string guid = "";
            if (sdr.Read())
            {
                guid = sdr.GetValue(0).ToString();
            }
            return guid;
        }

        internal static void SetStateClient(string guidClient, string state, string ip = "")
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"Update Client SET address=@ip where guid = @guid";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidClient);
            cmd.Parameters.AddWithValue("ip", ip);
            cmd.ExecuteNonQuery();
            cmd.Cancel();
            comm = @"Insert INTO [dbo].[ClientState] ([date],[clientId],[stateId]) VALUES (@date, (Select id from client where guid=@guid), @state)";
            cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guidClient);
            cmd.Parameters.AddWithValue("date", DateTime.Now);
            if (state == "active")
                cmd.Parameters.AddWithValue("state", 2);
            else if (state == "auth")
                cmd.Parameters.AddWithValue("state", 3);
            else
                cmd.Parameters.AddWithValue("state", 1);
            cmd.ExecuteNonQuery();
        }

        internal static void UpdateValue(string value, string prop)
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"Update Parametrs SET [value]=@val where [property] = @prop";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("val", value);
            cmd.Parameters.AddWithValue("prop", prop);
            cmd.ExecuteNonQuery();
        }

        internal static void ChangePassword(string newLogin, string newPass)
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"ALTER LOGIN @login WITH PASSWORD = @pass";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("pass", newPass);
            cmd.Parameters.AddWithValue("login", newLogin);
            cmd.ExecuteNonQuery();
        }

        internal static void SetCertificateStatus(string guid, string status)
        {
            var conn = GetConnection();
            if (conn == null)
                return;
            string comm = @"Insert INTO [dbo].[CertificateStatus] ([CertificateId],[statusId],[date]) VALUES ((Select id from [dbo].[Certificate] where guid=@guid), @state, @date)";
            SqlCommand cmd = new SqlCommand(comm, conn);
            cmd.Parameters.AddWithValue("guid", guid);
            cmd.Parameters.AddWithValue("date", DateTime.Now);
            if (status == "active")
                cmd.Parameters.AddWithValue("state", 2);
            else if (status == "block")
                cmd.Parameters.AddWithValue("state", 3);
            else if (status == "timeEnd")
                cmd.Parameters.AddWithValue("state", 4);
            else
                cmd.Parameters.AddWithValue("state", 1);
            cmd.ExecuteNonQuery();
            cmd = null;
        }

        //internal static Certificate GetCertificate(string guid)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"EXEC [dbo].[GetCertificate] @guid ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guid);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    Certificate cert = new Certificate();
        //    if (sdr.Read())
        //    {
        //        cert.dateCreating = sdr.GetDateTime(6);
        //        cert.dateStart = sdr.GetDateTime(4);
        //        cert.dateStop = sdr.GetDateTime(5);
        //        cert.publicKey = sdr.GetValue(2).ToString();// [0].publicKey;
        //        //text = text + sdr.GetValue(0).ToString();
        //    }
        //    return cert;
        //}

        //internal static string GetOnlineClientsForServer(string guid)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"Select sel.[address], [Program].port, [Program].protocol from 
        //                    (SELECT prof.id, usrsPrf.[address] FROM (SELECT ClientProfile.profileId, [LastClientsStates].[address] FROM [dbo].[LastClientsStates] 
	       //                     inner join ClientProfile on [LastClientsStates].clientId=ClientProfile.clientId WHERE [stateId]=2) AS usrsPrf 
		      //              left join (Select [Profile].id,[Profile].programid from [dbo].[Profile] where [serverId]=(Select [id] from [dbo].[Server] where [guid]=@guid)) as prof on usrsPrf.profileId=prof.id) as sel
		      //          inner join [dbo].[Program] on sel.id=Program.id";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guid);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    string address = "";
        //    while (sdr.Read())
        //    {
        //        if (address == "")
        //            address = sdr.GetValue(0).ToString() + ":" + sdr.GetValue(1).ToString() + ":" + sdr.GetValue(2).ToString();
        //        else
        //            address = address + ";" + sdr.GetValue(0).ToString() + ":" + sdr.GetValue(1).ToString() + ":" + sdr.GetValue(2).ToString();
        //    }
        //    return address;
        //}

        //internal static void InsertTempGuid(string guid, string clientGuid)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return;
        //    string comm = @"EXEC insertTempGuid @guid, @ClientGuid ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guid);
        //    cmd.Parameters.AddWithValue("ClientGuid", clientGuid);
        //    cmd.ExecuteNonQuery();
        //}

        //internal static string GetValue(string key)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"Select [value] from parametrs where [property]=@key";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("key", key);
        //    var reader = cmd.ExecuteReader();
        //    if (reader.Read())
        //        return reader.GetValue(0).ToString();
        //    return null;
        //}

        //internal static string GetPartKeyUsb(string guidClient)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"EXEC GetPartKey @guid ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guidClient);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    string key = "";
        //    if (sdr.Read())
        //    {
        //        key = sdr.GetValue(0).ToString();
        //    }
        //    return key;
        //}

        //internal static string CheckTocken(string goodData, string guidUsb)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"SELECT TempGuid.guid, TempGuid.date FROM  Client INNER JOIN Usb ON Client.usbId = Usb.id INNER JOIN TempGuid ON Client.id = TempGuid.clientId WHERE (Usb.guid = @guid) Order by TempGuid.id desc";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guidUsb);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    if (sdr.Read())
        //    {
        //        if (goodData == sdr.GetValue(0).ToString() && sdr.GetDateTime(1).AddMinutes(1) > DateTime.Now)
        //        {
        //            return "good";
        //        }
        //    }
        //    return "not good";
        //}

        //internal static Certificate GetCertificateUsbOfClient(string guidClient)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"EXEC [dbo].[GetUsbCertificateOfClient] @guid ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guidClient);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    Certificate cert = new Certificate();
        //    if (sdr.Read())
        //    {
        //        cert.dateCreating = sdr.GetDateTime(3);
        //        cert.dateStart = sdr.GetDateTime(1);
        //        cert.dateStop = sdr.GetDateTime(2);
        //        cert.publicKey = sdr.GetValue(0).ToString();// [0].publicKey;
        //        //text = text + sdr.GetValue(0).ToString();
        //    }
        //    return cert;
        //}

        //internal static Certificate GetUSBCertificate(string guidUsb)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return null;
        //    string comm = @"EXEC [dbo].[GetUsbCert] @guid ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("guid", guidUsb);
        //    SqlDataReader sdr = cmd.ExecuteReader();
        //    Certificate cert = new Certificate();
        //    if (sdr.Read())
        //    {
        //        //cert.dateCreating = sdr.GetValue[0].date;
        //        //cert.dateStart = rez[0].dateStart;
        //        //cert.dateStop = rez[0].dateStop;
        //        cert.publicKey = sdr.GetValue(1).ToString();// [0].publicKey;
        //        //text = text + sdr.GetValue(0).ToString();
        //    }
        //    return cert;
        //}

        //internal static void AddLogs(string name, string text, TypeLog type)
        //{
        //    var conn = GetConnection();
        //    if (conn == null)
        //        return;
        //    string comm = @"INSERT INTO [Log] VALUES (@name,@text,@date,@type) ";
        //    SqlCommand cmd = new SqlCommand(comm, conn);
        //    cmd.Parameters.AddWithValue("name", name);
        //    cmd.Parameters.AddWithValue("text", text);
        //    cmd.Parameters.AddWithValue("date", DateTime.Now);
        //    cmd.Parameters.AddWithValue("type", type);
        //    cmd.ExecuteNonQuery();
        //}
    }
}
