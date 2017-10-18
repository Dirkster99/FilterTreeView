namespace BusinessLib.Models
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;

    public class MetaLocationModel
    {
        #region fields
        private readonly ObservableCollection<MetaLocationModel> _Children = null;
        #endregion fields

        #region constructors
        /// <summary>
        /// Parameterized Class Constructor
        /// </summary>
        public MetaLocationModel(
              MetaLocationModel parent
            , int id
            , string iso
            , string localName
            , LocationType type
            , long in_Location
            , double geo_lat
            , double geo_lng
            , string db_id
            )
            : this()
        {
            Parent = parent;

            ID = id;
            ISO = iso;
            LocalName = localName;
            Type = type;
            In_Location = in_Location;
            Geo_lat = geo_lat;
            Geo_lng = geo_lng;
            DB_id = db_id;
        }

        /// <summary>
        /// Class Constructor
        /// </summary>
        public MetaLocationModel()
        {
            _Children = new ObservableCollection<MetaLocationModel>();
        }
        #endregion constructors

        #region properties
        [XmlIgnore]
        public MetaLocationModel Parent { get; private set; }

        public int ID { get; set; }
        public string ISO { get; set; }
        public string LocalName { get; set; }
        public LocationType Type { get; set; }

        public long In_Location { get; set; }

        public double Geo_lat { get; set; }
        public double Geo_lng { get; set; }

        public string DB_id { get; set; }

        [XmlIgnore]
        public IEnumerable<MetaLocationModel> Children
        {
            get
            {
                return _Children;
            }
        }
        #endregion properties

        #region methods
        public int ChildrenCount => _Children.Count;

        public void ChildrenAdd(MetaLocationModel child)
        {
            _Children.Add(child);
        }

        public void ChildrenRemove(MetaLocationModel child)
        {
            _Children.Remove(child);
        }

        public void ChildrenClear()
        {
            _Children.Clear();
        }

        public void SetParent(MetaLocationModel parent)
        {
            Parent = parent;
        }
        #endregion methods
    }
}
