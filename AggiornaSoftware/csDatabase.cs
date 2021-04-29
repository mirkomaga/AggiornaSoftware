// (C) Copyright 2019 by Focchi SpA - Italy
//
// FOCCHI SPA PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// FOCCHI SPA SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. 
// FOCCHI SPA DOES NOT WARRANT THAT THE OPERATION OF THE
// PROGRAM WILL BE UNINTERRUPTED OR ERROR FREE.
//
// Description:
// Procedure di gestione Database di progetto
//
// History:
// 24.11.2008 [MC] Prima stesura
// 19.02.2019 [MC] Conversione per Focchi e SQL Server
// 12.04.2021 [MM] Inserimento logiche lock record


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace AggiornaSoftware
{
    public enum eDbType
    {
        MySql = 0,
        MSSQL = 1
    }

    public struct ids
    {
        public String szID;
        public int iQta;
    };

    public class csDatabase
    {
        // tipo elemento
        public const int ET_UNDEFINED = 0;
        public const int ET_PANNELLO = 1;
        public const int ET_PROFILO = 2;
        public const int ET_CELLULA = 3;

        // UM x costo tender
        public const int UMC_NUMBER = 0;
        public const int UMC_LENGTH = 1;
        public const int UMC_AREA = 2;

        public const String UMC_NUMBER_DESCR = "€/pz";
        public const String UMC_LENGTH_DESCR = "€/m";
        public const String UMC_AREA_DESCR = "€/mq";
        public const String UMC_HOUR_DESCR = "€/h";



        // stringhe costanti interne
        // costanti default connessione MySQL
        public const string MYCONNECTIONSTRINGNODB = @"server={0};Port={1};user id={2};password={3};persist security info=True;SslMode=none;AllowPublicKeyRetrieval=true;";
        public const string MYCONNECTIONSTRING = @"server={0};Port={1};user id={2};password={3};persist security info=True;database={4};SslMode=none;AllowPublicKeyRetrieval=true;";

        public const string MYSQL_DEFAULT_PORT = "3306";
        public const string MSSQL_DEFAULT_PORT = "1433";

        //public const string MYSQL_SERVER = @"192.168.100.2";    // server
        //public const string MYSQL_PORT = @"3306";               // server service port
        //public const string MYSQL_USER = @"root";               // utente di connessione
        //public const string MYSQL_PASSWORD = @"Quink$2100";     // password


        // connessione per MS-SQL Server
        const String CONNECTIONSTRINGNODB =
            "Server=tcp:{0}, {1};" +
            "User ID={2};" +
            "Password={3};" + 
            "Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";

        const String CONNECTIONSTRING =
            "Server=tcp:{0}, {1};" +
            "Database={4};" +
            "User ID={2};" +
            "Password={3};" +
            "Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";

        // ? Stringa di connessione server
        const String SERVERCONNECTIONSTRING = "server={0};user={1};port=3306;password={2};";


        public const String BIM = "aggiornamentoPlugins";
        public const String BIM_TEMPLATE = "bim_template";


        // configurazione di accesso a DB
        //public static String szServer = "SRVSQL";
        //public static String szUser ="bim";                 // "sa"
        //public static String szPassword = "quink2100";      // "fonera2.0"
        //public static String szPort = @"1433";

        public static String szServer = "192.168.100.2";
        public static String szUser = "bim";
        public static String szPassword = "Quink$2100";
        public static String szPort = @"3306";

        // catalogo db
        public static List<String> m_dbcatalog = new List<String>();


        // dichiarazioni statiche pubbliche
        public static List<String> m_lstReservedDBs = new List<String>(new String[] { "MASTER", "MODEL", "MSDB", "TEMPDB", "TEMPLATE", "FOCCHI", "BIM", 
                                                                                      "MYSQL", "INFORMATION_SCHEMA", "PERFORMANCE_SCHEMA", "TEST", "EDGE", "SYS" });

        public static eDbType dbType = eDbType.MySql;       // tipo di database connesso

        String m_DbName;                        // nome database

        // connessione per MS-SQL Server
        SqlConnection m_msconnection;           // connessione al DB
        SqlTransaction m_mstransaction;         // transazione aperta

        // connessione per MySQL
        MySqlConnection m_myconnection;         // connessione al DB
        MySqlTransaction m_mytransaction;       // transazione aperta


        // stato e ultimo messaggio di errore
        bool m_bHasError;
        String m_szErrorMessage;
        String m_szLastSQL;

        // ctor: default, richiede InitDB esplicito
        public csDatabase()
        {
            m_msconnection = null;
            m_mstransaction = null;

            m_myconnection = null;
            m_mytransaction = null;

            m_DbName = "";

            ResetError();
        }

        // ctor: inizializza con percorso e nome corretto
        public csDatabase(String szDbName)
        {
            ResetError();

            m_msconnection = null;
            m_mstransaction = null;

            m_myconnection = null;
            m_mytransaction = null;

            m_DbName = CleanDbName(szDbName);

            InitDb();

            if (!IsConnected)
            {
                checkDB();
            }
        }

        ~csDatabase()
        {
            this.Close();
        }

        public static String CleanDbName(String szName)
        {
            String result = "";

            foreach (Char c in szName)
            {
                if (Char.IsControl(c)) continue;
                if (Char.IsWhiteSpace(c)) continue;
                if (c == '.') result += '_';
                else result += c;
            }

            return result;
        }

        private void checkDB()
        {
            string connStr = String.Format(SERVERCONNECTIONSTRING, szServer, szUser, szPassword);

            using (MySqlConnection conn = new MySqlConnection(connStr))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{DbName}`;";
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            InitDb();
        }

        public static String CheckConnection(eDbType dbtype, String szServer, String szPort, String szUser, String szPassword)
        {
            String result = "";

            try
            {
                if (csDatabase.dbType == eDbType.MySql)
                {
                    // in caso di porta non definita, utilizza default
                    if (String.IsNullOrEmpty(szPort)) szPort = MYSQL_DEFAULT_PORT;

                    // componi stringa di connessione e tenta apertura
                    String szConnString = String.Format(MYCONNECTIONSTRINGNODB, szServer, szPort, szUser, szPassword);

                    MySqlConnection conn = new MySqlConnection(szConnString);
                    conn.Open();

                    if (conn.State == ConnectionState.Open) result = "MySQL Server version " + conn.ServerVersion;

                    conn.Close();
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // in caso di porta non definita, utilizza default
                    if (String.IsNullOrEmpty(szPort)) szPort = MSSQL_DEFAULT_PORT;

                    // componi stringa di connessione e tenta apertura
                    String szConnString = String.Format(CONNECTIONSTRINGNODB, szServer, szPort, szUser, szPassword);

                    SqlConnection conn = new SqlConnection(szConnString);
                    conn.Open();

                    if (conn.State == ConnectionState.Open) result = "Microsoft SQL Server version " + conn.ServerVersion;

                    conn.Close();
                }
            }
            catch (System.Exception ex)
            {
                result = "Connection Error! " + ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Tabelle database
        /// </summary>
        /// <returns>Lista nomi tabelle presenti nel database istanziato</returns>
        public List<string> getTablesName()
        {
            List<string> tables = new List<string>();

            if (csDatabase.dbType == eDbType.MySql)
            {
                if (!IsConnected) return tables;
                if (m_myconnection == null) return tables;

                DataTable dtTables = m_myconnection.GetSchema("Tables");

                foreach (DataRow row in dtTables.Rows)
                {
                    tables.Add((string)row["TABLE_NAME"]);
                }
            }

            return tables;
        }


        /// <summary>
        /// Popola un dataset con il database istanziato
        /// </summary>
        /// <returns>Datase del database completo</returns>
        internal DataSet getDataset(List<string> filterTable = null)
        {
            DataSet ds = new DataSet();

            if (csDatabase.dbType == eDbType.MySql)
            { 
                if (!IsConnected) return ds;
                if (m_myconnection == null) return ds;

                DataTable dtTables = m_myconnection.GetSchema("Tables");
                
                foreach (string tableName in getTablesName())
                {
                    if (filterTable != null && !filterTable.Contains(tableName)) continue;

                    string queryString = $"SELECT * FROM {tableName};";

                    // crea comando con query di selezione
                    MySqlDataAdapter adapter = new MySqlDataAdapter(queryString, m_myconnection);

                    adapter.Fill(ds, tableName);
                }
            }

            return ds;
        }

        internal void addRecordToUsernameAggiornamento(csOperazione csOperazione)
        {
            string query = $@"INSERT INTO `usernameaggiornamento` (`uaUtente`, `uaPathSoftware`, `uaDataSoftware`, `uaDateTime`, `uaStatus`, `uaAction`) VALUES('{csOperazione.m_Utente}','{csOperazione.m_Software}', {cvField2Data(csOperazione.m_DataSoftware)}, NOW(),{csOperazione.m_Status}, '{csOperazione.m_Action}');";


            int id = this.ExecuteSqlInsert(query);
        }

        public bool IsConnected
        {
            get
            {
                if (csDatabase.dbType == eDbType.MySql)
                {
                    if (m_myconnection != null)
                        if (m_myconnection.State == ConnectionState.Open)
                            return true;
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    if (m_msconnection != null)
                        if (m_msconnection.State == ConnectionState.Open)
                            return true;
                }

                return false;
            }
        }

        public bool HasError
        {
            get
            {
                return m_bHasError;
            }
        }

        public String ErrorMessage
        {
            get
            {
                return m_szErrorMessage;
            }
        }

        public String LastSQL
        {
            get
            {
                return m_szLastSQL;
            }
        }

        public String DbName
        {
            get
            {
                return m_DbName;
            }
        }

        private void ResetError()
        {
            m_bHasError = false;
            m_szErrorMessage = "";
        }

        private void SetError(String szErrorMessage)
        {
            m_szErrorMessage = szErrorMessage;
            m_bHasError = true;
        }

        private void SetError(String szErrorMessage, String szSQLStatement)
        {
            m_szErrorMessage = szErrorMessage;
            m_szLastSQL = szSQLStatement;
            m_bHasError = true;
        }

        public void Close()
        {
            ResetError();

            // controlla ed eventualmente chiudi precedente connessione
            if (m_msconnection != null)
            {
                if (m_msconnection.State != ConnectionState.Closed)
                {
                    try
                    {
                        // se c'è una transazione in attesa esegui Commit()
                        if (m_mstransaction != null) m_mstransaction.Commit();

                        m_msconnection.Close();
                    }
                    catch (System.Exception ex)
                    {
                        SetError(ex.Message);
                    }
                }
                m_msconnection.Dispose();

                m_mstransaction = null;
                m_msconnection = null;
            }

            if (m_myconnection != null)
            {
                if (m_myconnection.State != ConnectionState.Closed)
                {
                    try
                    {
                        // se c'è una transazione in attesa esegui Commit()
                        if (m_mytransaction != null) m_mytransaction.Commit();

                        m_myconnection.Close();
                    }
                    catch (System.Exception ex)
                    {
                        SetError(ex.Message);
                    }
                }
                m_myconnection.Dispose();

                m_mytransaction = null;
                m_myconnection = null;
            }
        }


        /// <summary>
        /// Apri nuova transazione per la connessione corrente.
        /// Non sono supportate transazioni nidificate.
        /// </summary>
        /// <returns></returns>
        public bool StartTransaction()
        {
            if (!IsConnected) return false;

            bool result = false;

            // azzera precedenti errori
            ResetError();

            try
            {
                if (csDatabase.dbType == eDbType.MySql)
                {
                    // chiudi eventuale transazione aperta
                    if (m_mytransaction != null)
                        m_mytransaction.Commit();

                    // apri nuova transazione
                    m_mytransaction = m_myconnection.BeginTransaction();
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // chiudi eventuale transazione aperta
                    if (m_mstransaction != null)
                        m_mstransaction.Commit();

                    // apri nuova transazione
                    m_mstransaction = m_msconnection.BeginTransaction();
                }

                result = true;
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Concludi transazione aperta
        /// </summary>
        /// <returns></returns>
        public bool Commit()
        {
            if (!IsConnected) return false;

            bool result = false;

            // azzera precedenti errori
            ResetError();

            try
            {
                if (csDatabase.dbType == eDbType.MySql)
                {
                    // chiudi transazione aperta
                    if (m_mytransaction != null)
                        m_mytransaction.Commit();

                    m_mytransaction = null;
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // chiudi transazione aperta
                    if (m_mstransaction != null)
                        m_mstransaction.Commit();

                    m_mstransaction = null;
                }

                result = true;
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Annulla transazione aperta
        /// </summary>
        /// <returns></returns>
        public bool Rollback()
        {
            if (!IsConnected) return false;

            bool result = false;

            // azzera precedenti errori
            ResetError();

            try
            {
                if (csDatabase.dbType == eDbType.MySql)
                {
                    // annulla transazione aperta
                    if (m_mytransaction != null)
                        m_mytransaction.Rollback();

                    m_mytransaction = null;
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // annulla transazione aperta
                    if (m_mstransaction != null)
                        m_mstransaction.Rollback();

                    m_mstransaction = null;
                }
                result = true;
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Inizializza database aprendo la connessione al database di progetto specificandone i parametri.
        /// </summary>
        /// <param name="szProjectPath"></param>
        /// <param name="szProjectName"></param>
        /// <returns></returns>
        public bool InitDb(String szProjectName)
        {
            m_DbName = CleanDbName(szProjectName);
            if (m_DbName.Length == 0) return false;      // nome non valido

            InitDb();

            return IsConnected;
        }

        /// <summary>
        /// Inizializza database aprendo la connessione al database di progetto.
        /// </summary>
        /// <returns></returns>
        public bool InitDb()
        {
            ResetError();

            // controlla ed eventualmente chiudi precedente connessione
            Close();

            // crea nuova connessione
            String szConnString = "";

            if (m_DbName.Length > 0)
            {
                // connetti e apri DB
                if (csDatabase.dbType == eDbType.MySql)
                    szConnString = String.Format(MYCONNECTIONSTRING, szServer, szPort, szUser, szPassword, m_DbName);

                else if (csDatabase.dbType == eDbType.MSSQL)
                    szConnString = String.Format(CONNECTIONSTRING, szServer, szPort, szUser, szPassword, m_DbName);
            }
            else
            {
                // connetti semplicemente, per operazioni sul server
                if (csDatabase.dbType == eDbType.MySql)
                    szConnString = String.Format(MYCONNECTIONSTRINGNODB, szServer, szPort, szUser, szPassword);

                else if (csDatabase.dbType == eDbType.MSSQL)
                    szConnString = String.Format(CONNECTIONSTRINGNODB, szServer, szPort, szUser, szPassword);
            }

            try
            {
                m_myconnection = null;
                m_msconnection = null;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    m_myconnection = new MySqlConnection(szConnString);
                    m_myconnection.Open();
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    m_msconnection = new SqlConnection(szConnString);
                    m_msconnection.Open();
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message);
            }

            return IsConnected;
        }


        /// <summary>
        /// Esegui comando Sql che non prevede restituzione di informazioni
        /// </summary>
        /// <param name="szSqlCommand"></param>
        /// <returns></returns>
        public bool ExecuteSqlCommand(String szSqlCommand)
        {
            bool result = true;

            ResetError();

            try
            {
                m_szLastSQL = szSqlCommand;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    MySqlCommand cmd = new MySqlCommand(szSqlCommand, m_myconnection);
                    if (m_mytransaction != null) cmd.Transaction = m_mytransaction;
                    cmd.CommandTimeout = 600;

                    cmd.ExecuteNonQuery();
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando
                    SqlCommand cmd = new SqlCommand(szSqlCommand, m_msconnection);
                    if (m_mstransaction != null) cmd.Transaction = m_mstransaction;
                    cmd.CommandTimeout = 600;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szSqlCommand);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Esegui comando Sql che prevede la restituzione di un solo valore
        /// </summary>
        /// <param name="szSqlCommand"></param>
        /// <returns></returns>
        public Object ExecuteScalar(String szSqlCommand)
        {
            // controlli generici
            Object result = null;

            ResetError();

            try
            {
                m_szLastSQL = szSqlCommand;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea comando
                    MySqlCommand cmd = new MySqlCommand(szSqlCommand, m_myconnection);
                    if (m_mytransaction != null) cmd.Transaction = m_mytransaction;
                    cmd.CommandTimeout = 600;

                    result = cmd.ExecuteScalar();
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando
                    SqlCommand cmd = new SqlCommand(szSqlCommand, m_msconnection);
                    if (m_mstransaction != null) cmd.Transaction = m_mstransaction;
                    cmd.CommandTimeout = 600;

                    result = cmd.ExecuteScalar();
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szSqlCommand);
            }

            return result;
        }

        /// <summary>
        /// Esegui comando Sql di inserimento.
        /// Ritorna l'ID appena creato in caso di colonne ID di AutoIncremento
        /// </summary>
        /// <param name="szSqlCommand"></param>
        /// <returns></returns>
        public int ExecuteSqlInsert(String szSqlCommand)
        {
            // controlli generici
            if (!IsConnected) return 0;
            if (szSqlCommand.Length == 0) return 0;

            int result = 0;

            ResetError();

            try
            {
                m_szLastSQL = szSqlCommand;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea ed esegui comando
                    MySqlCommand cmd = new MySqlCommand(szSqlCommand, m_myconnection);
                    if (m_mytransaction != null) cmd.Transaction = m_mytransaction;
                    cmd.CommandTimeout = 600;
                    cmd.ExecuteNonQuery();

                    // recupera nuovo ID appena generato
                    MySqlCommand getID = new MySqlCommand("SELECT @@IDENTITY", m_myconnection, m_mytransaction);
                    result = Convert.ToInt32(getID.ExecuteScalar());
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea ed esegui comando
                    SqlCommand cmd = new SqlCommand(szSqlCommand, m_msconnection);
                    if (m_mstransaction != null) cmd.Transaction = m_mstransaction;
                    cmd.CommandTimeout = 600;
                    cmd.ExecuteNonQuery();

                    // recupera nuovo ID appena generato
                    SqlCommand getID = new SqlCommand("SELECT @@IDENTITY", m_msconnection, m_mstransaction);
                    result = Convert.ToInt32(getID.ExecuteScalar());
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szSqlCommand);
            }

            return result;
        }

        /// <summary>
        /// Esegui query di selezione e ritorna nuovo DataSet contenente i dati recuperati
        /// </summary>
        /// <param name="szQuery">Query da eseguire</param>
        /// <returns>Ritorna un nuovo DataSet contenente i dati selezionati</returns>
        public DataSet ReadData(String szQuery)
        {
            // controlli generici
            if (!IsConnected) return null;
            if (szQuery.Length == 0) return null;

            // azzera precedenti errori e inizializza dataset di ritorno
            ResetError();
            DataSet ds = null;

            try
            {
                m_szLastSQL = szQuery;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea comando con query di selezione
                    MySqlDataAdapter da = new MySqlDataAdapter(szQuery, m_myconnection);
                    da.SelectCommand.CommandTimeout = 600;
                    da.FillError += new FillErrorEventHandler(da_FillError);

                    // crea DataSet e ritorna nuovo DataSet riempito con i dati della query
                    ds = new DataSet();
                    da.Fill(ds);
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando con query di selezione
                    SqlDataAdapter da = new SqlDataAdapter(szQuery, m_msconnection);
                    da.SelectCommand.CommandTimeout = 600;
                    da.FillError += new FillErrorEventHandler(da_FillError);

                    // crea DataSet e ritorna nuovo DataSet riempito con i dati della query
                    ds = new DataSet();
                    da.Fill(ds);
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message);
            }

            return ds;
        }

        /// <summary>
        /// Esegui query di selezione e riempi DataSet contenente i dati recuperati
        /// </summary>
        /// <param name="szQuery">Query da eseguire</param>
        /// <param name="ds">Dataset da riempire</param>
        /// <returns>Flag di buona riuscita</returns>
        public bool ReadData(String szQuery, DataSet ds)
        {
            // controlli generici
            if (!IsConnected) return false;
            if (szQuery.Length == 0) return false;
            if (ds == null) ds = new DataSet();

            bool result = true;

            // azzera precedenti errori
            ResetError();

            try
            {
                m_szLastSQL = szQuery;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea comando con query di selezione
                    MySqlDataAdapter da = new MySqlDataAdapter(szQuery, m_myconnection);
                    da.SelectCommand.CommandTimeout = 600;
                    da.FillError += new FillErrorEventHandler(da_FillError);

                    // riempi dataset con i dati della query
                    da.Fill(ds);
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando con query di selezione
                    SqlDataAdapter da = new SqlDataAdapter(szQuery, m_msconnection);
                    da.SelectCommand.CommandTimeout = 600;
                    da.FillError += new FillErrorEventHandler(da_FillError);

                    // riempi dataset con i dati della query
                    da.Fill(ds);
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szQuery);
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Esegui query di selezione e riempi DataTable contenente i dati recuperati
        /// </summary>
        /// <param name="szQuery">Query da eseguire</param>
        /// <param name="dt">DataTable da riempire</param>
        /// <returns>Flag di buona riuscita</returns>
        public bool ReadData(String szQuery, System.Data.DataTable dt)
        {
            // controlli generici
            if (!IsConnected) return false;
            if (szQuery.Length == 0) return false;
            if (dt == null) dt = new DataTable();

            bool result = true;

            // azzera precedenti errori
            ResetError();

            try
            {
                m_szLastSQL = szQuery;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea comando con query di selezione
                    MySqlDataAdapter da = new MySqlDataAdapter(szQuery, m_myconnection);
                    da.FillError += new FillErrorEventHandler(da_FillError);
                    da.SelectCommand.CommandTimeout = 600;

                    // riempi dataset con i dati della query
                    da.Fill(dt);
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando con query di selezione
                    SqlDataAdapter da = new SqlDataAdapter(szQuery, m_msconnection);
                    da.FillError += new FillErrorEventHandler(da_FillError);
                    da.SelectCommand.CommandTimeout = 600;

                    // riempi dataset con i dati della query
                    da.Fill(dt);
                }
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szQuery);
                result = false;
            }

            return result;
        }

        void da_FillError(object sender, FillErrorEventArgs e)
        {
            SetError(e.Errors.Message);
            e.Continue = true;
        }

        static void da_SkipError(object sender, FillErrorEventArgs e)
        {
            e.Continue = true;
        }

        /// <summary>
        /// Leggi schema e definisci tabella in base alla query di selezione
        /// </summary>
        /// <param name="szQuery"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public bool ReadSchema(String szQuery, System.Data.DataTable dt)
        {
            bool result = false;

            // controlli generici
            if (!IsConnected) return false;
            if (szQuery.Length == 0) return false;
            if (dt == null) return false;

            // azzera precedenti errori
            ResetError();

            try
            {
                m_szLastSQL = szQuery;

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea comando con query di selezione
                    MySqlDataAdapter da = new MySqlDataAdapter(szQuery, m_myconnection);
                    da.SelectCommand.CommandTimeout = 600;

                    // riempi dataset con i dati della query
                    da.FillSchema(dt, SchemaType.Source);
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando con query di selezione
                    SqlDataAdapter da = new SqlDataAdapter(szQuery, m_msconnection);
                    da.SelectCommand.CommandTimeout = 600;

                    // riempi dataset con i dati della query
                    da.FillSchema(dt, SchemaType.Source);
                }
                result = true;
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szQuery);
            }

            return result;
        }

        internal void deleteteDatabase()
        {
            string connStr = String.Format(SERVERCONNECTIONSTRING, szServer, szUser, szPassword);

            using (MySqlConnection conn = new MySqlConnection(connStr))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"DROP DATABASE IF EXISTS `{DbName}`;";
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        /// <summary>
        /// Compresses a string and returns a deflate compressed, Base64 encoded string.
        /// </summary>
        /// <param name="uncompressedString">String to compress</param>
        public static String Compress(String uncompressedString)
        {
            byte[] compressedBytes;

            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
            {
                using (var compressedStream = new MemoryStream())
                {
                    // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                    // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                    // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return Convert.ToBase64String(compressedBytes);
        }

        /// <summary>
        /// Decompresses a deflate compressed, Base64 encoded string and returns an uncompressed string.
        /// </summary>
        /// <param name="compressedString">String to decompress.</param>
        public static String Decompress(String compressedString)
        {
            byte[] decompressedBytes;

            var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

            using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    decompressorStream.CopyTo(decompressedStream);

                    decompressedBytes = decompressedStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decompressedBytes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// helper functions


        /// <summary>
        /// Helper function: recupera prima riga di una query di selezione.
        /// Utile nel caso si debba recuperare una sola riga precisa con chiave univoca.
        /// </summary>
        /// <param name="szQuery">Query di selezione con filtro su chiave univoca.</param>
        /// <returns></returns>
        public System.Data.DataRow GetFirstRow(String szQuery)
        {
            System.Data.DataRow result = null;

            // controlli generici
            if (!IsConnected) return null;
            if (szQuery.Length == 0) return null;

            // azzera precedenti errori
            ResetError();

            try
            {
                m_szLastSQL = szQuery;

                System.Data.DataTable dt = new System.Data.DataTable();

                if (csDatabase.dbType == eDbType.MySql)
                {
                    // crea comando con query di selezione
                    MySqlDataAdapter da = new MySqlDataAdapter(szQuery, m_myconnection);

                    // riempi dataset con i dati della query
                    da.Fill(dt);
                }
                else if (csDatabase.dbType == eDbType.MSSQL)
                {
                    // crea comando con query di selezione
                    SqlDataAdapter da = new SqlDataAdapter(szQuery, m_msconnection);

                    // riempi dataset con i dati della query
                    da.Fill(dt);
                }

                // recupera prima (e forse unica) riga della DataTable
                if (dt.Rows.Count > 0) result = dt.Rows[0];
            }
            catch (System.Exception ex)
            {
                SetError(ex.Message, szQuery);
            }

            return result;
        }



        /// <summary>
        /// Traduci elementi dictionary Key-Value in stringa unica per scrittura in campo di db
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static String SerializeSymbols(SortedDictionary<String, Object> dictionary)
        {
            String result = "";

            if (dictionary != null)
            {
                foreach (KeyValuePair<String, Object> itm in dictionary)
                {
                    if (itm.Key.Trim().Length == 0) continue;

                    if (itm.Value is Double)
                    {
                        result += itm.Key + "=" + Convert.ToString(Math.Round((Double)itm.Value, 5), System.Globalization.CultureInfo.InvariantCulture) + "¦";
                    }
                    else if ((itm.Value is Int32) || (itm.Value is Int16) || (itm.Value is Int64))
                    {
                        result += itm.Key + "=" + itm.Value.ToString() + "¦";
                    }
                    else
                        result += itm.Key + "='" + Convert.ToString(itm.Value, System.Globalization.CultureInfo.InvariantCulture) + "'|";
                }

                if (result.Length > 8000) result = result.Substring(0, 8000);
            }

            return result;
        }

        /// <summary>
        /// Converti stringa formattata chiavi=valori in Dictionary
        /// </summary>
        /// <param name="szSymbols"></param>
        /// <param name="m_symbols"></param>
        public static void DeSerializeSymbols(String szSymbols, ref SortedDictionary<String, Object> m_symbols)
        {
            if (m_symbols == null) m_symbols = new SortedDictionary<String, Object>();
            else m_symbols.Clear();

            String[] v = szSymbols.Trim().Split('¦');
            foreach (String sk in v)
            {
                if (sk.Length > 0)
                {
                    int p = sk.IndexOf('=');
                    if (p > 0)
                    {
                        String szValue = sk.Substring(p + 1);
                        if (szValue.StartsWith("'") && szValue.EndsWith("'"))
                        {
                            // testo esplicito
                            m_symbols.Add(sk.Substring(0, p), szValue.Substring(1, szValue.Length - 2));
                        }
                        else
                        {
                            // tenta riconoscimento grandezza numerica
                            Double d;
                            if (Double.TryParse(szValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                                m_symbols.Add(sk.Substring(0, p), d);
                            else
                                m_symbols.Add(sk.Substring(0, p), szValue);
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Conversione generica da campo dati Intero in stringa di testo per la
        /// composizione di Sql di inserimento o aggiornamento
        /// </summary>
        /// <param name="sField"></param>
        /// <returns></returns>
        public static string cvField2int(object sField)
        {
            string retVal = "0";

            if (sField != null)
            {
                // prova conversione, altrimenti usa default
                try
                {
                    retVal = Convert.ToInt32(sField).ToString();
                }
                catch
                {
                    retVal = "0";
                }
            }
            return retVal;
        }



        /// <summary>
        /// Conversione generica in campo numerico generico per la composizione
        /// di query di aggiornamento o inserimento
        /// </summary>
        /// <param name="sField"></param>
        /// <returns></returns>
        public static string cvField2num(object sField)
        {
            string retVal = "0";

            if (sField != null)
            {
                // prova conversione, altrimenti usa default
                try
                {
                    retVal = sField.ToString().Replace(',', '.');
                    if (retVal.Length == 0) retVal = "0";
                }
                catch
                {
                    retVal = "0";
                }
            }
            return retVal;
        }

        /// <summary>
        /// Conversione generica campo Data/ora in formato testo per la composizione
        /// di Sql di inserimento o aggiornamento.
        /// </summary>
        /// <param name="sField"></param>
        /// <returns></returns>
        public static string cvField2Data(object sField)
        {
            string result = "null";

            if (sField != null)
            {
                DateTime dt;

                // prova conversione, altrimenti 'null' in caso di errore
                try
                {
                    dt = Convert.ToDateTime(sField);

                    // formato Access: formatta in {ts 'yyyy-mm-dd hh:mm:ss'}
                    //result = "{ts '" + dt.Year.ToString("0000") + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + " "
                    //                 + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00") + "'}";

                    if (csDatabase.dbType == eDbType.MySql)
                    {
                        // Versione per MySQL: formatta in YYYY-MM-DD HH:MM:SS
                        result = String.Format("'{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}'", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    }
                    else if (csDatabase.dbType == eDbType.MSSQL)
                    {
                        // Formato per SQL Server: YYYY-MM-DDThh:mm:ss (ISO 8601 format)
                        result = String.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    }

                    // formato DBMaker "'DD.MM.YYYY'd"
                    //szFld = dt.ToString().Substring(0, 10);
                    //szFld = "'" + szFld.Replace("/", ".") + "'d";
                }
                catch
                {
                    result = "null";
                }
            }
            return result;
        }

        /// <summary>
        /// Conversione generica campo di DataRow indicandone il tipo, come specificato in DataRow.Columns[].DataType
        /// </summary>
        /// <param name="fld"></param>
        /// <param name="fld_type"></param>
        /// <returns></returns>
        static private string convField(object fld, Type fld_type, eDbType dbtype)
        {
            string szFld = "";

            if (fld_type == System.Type.GetType("System.String"))
            {
                // testo, delimina tra apici singoli
                szFld += "'" + SostituisciApice(fld.ToString()) + "'";
            }
            else if (fld_type == System.Type.GetType("System.DateTime"))
            {
                DateTime dt = (DateTime)fld;    // short reference

                // Formato data Access "{ts 'YYYY-MM-DD HH:MM:SS'}"
                //szFld = String.Format("{ts '{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}'}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                if (dbtype == eDbType.MySql)
                {
                    // Formato data MySql Server "'YYYY-MM-DD HH:MM:SS'"
                    szFld = String.Format("'{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}'", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                }
                else if (dbtype == eDbType.MSSQL)
                {
                    // Formato data SQL Server "'YYYY-MM-DDTHH:MM:SS'"
                    szFld = String.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                }

                // formato DBMaker "'DD.MM.YYYY'd"
                //szFld = dt.ToString().Substring(0, 10);
                //szFld = "'" + szFld.Replace("/", ".") + "'d";
            }
            else
            {
                // numerico generico
                szFld = fld.ToString().Replace(",", ".");
                if (szFld.Length == 0) szFld = "0";
            }

            return szFld;
        }

        /// <summary>
        ///	Raddoppia apici in testi destinati a query SQL
        /// </summary>
        /// <param name="strIn">stringa con apici da sostituire</param>
        /// <returns>stringa con eventuali apici raddoppiati</returns>
        public static String SostituisciApice(String strIn)
        {
            //return strIn.Replace("'", "''");        // stringhe MSAccess, SQL Server
            if (strIn == null) return "";
            return strIn.Replace("'", "''");        // stringhe MySQL
        }

    }
}
