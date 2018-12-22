using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2CAppGraph
{
    /// <summary>
    /// ODataList wrapper which will be used when response contains a list of objects.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the collection that contains objects will be returned in the response
    /// </typeparam>
    public class ODataListWrapper<T>
        where T : IEnumerable
    {
        /// <summary>
        /// Gets or sets the value which contains a list of objects
        /// </summary>
        [JsonProperty]
        public T Value { get; set; }
    }
}
