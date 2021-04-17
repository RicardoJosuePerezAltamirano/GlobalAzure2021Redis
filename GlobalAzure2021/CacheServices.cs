using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalAzure2021
{
    public class CacheServices
    {
        

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {

            var config = Startup.StaticConfiguration;
            var value=config.GetValue<string>("CacheConnection"); //ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(value);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                /*
                 * StackExchange.Redis usa una opción de configuración llamada synctimeout para operaciones sincrónicas, con un valor predeterminado de 5000 ms. Si no se completa una llamada sincrónica en este tiempo, el cliente de StackExchange.Redis genera un error de tiempo de expiración 
                 * */
                System.Diagnostics.Debug.WriteLine($"conexion {lazyConnection.Value.TimeoutMilliseconds}");
                return lazyConnection.Value;
            }
        }
    }
}
