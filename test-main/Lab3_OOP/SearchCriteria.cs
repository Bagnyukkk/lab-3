using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3_OOP
{
    //клас призначений для критерії пошуку
    internal class SearchCriteria
    {
        public SearchCriteria()
        {
            AuthorName = "";
            Faculty = "";
            CustomerName = "";
            Branch = "";
        }

        public string AuthorName { get; set; }
        public string Faculty { get; set; }
        public string CustomerName { get; set; }
        public string Branch { get; set; }
    }
}
