using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AirFlight.Models
{
    public class KiasuContext : DbContext
    {
        public DbSet<Val> Vals { get; set; }

    }
    public class Val
    {
        [Key] //так как EF необходим первичный ключ, а в таблице Vals в базе данных nurba сервера kiasu ключ не указан, то мы используем свойство [key] пространсва имён System.ComponentModel.DataAnnotations; 
        public DateTime time { get; set; }       
        public int tag_id { get; set; }
        public float? val { get; set; }

    }

   
}