using System;

namespace AggiornaSoftware
{
    internal class csOperazione
    {
        public csOperazione()
        {
        }

        public string m_Utente { get; set; }
        public string m_Software { get; set; }
        public DateTime m_DataSoftware { get; set; }
        public bool m_Status { get; set; }
        public string m_Action { get; set; }
    }
}