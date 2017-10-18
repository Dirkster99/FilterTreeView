namespace BusinessLib
{
    using BusinessLib.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    /// <summary>
    /// A data source that provides raw data objects.  In a real
    /// application this class could also make calls to a database.
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Loads country, region, city data from an open source archiv:
        /// lokasyon.sql.gz dated back to 2012.
        /// Source: https://code.google.com/archive/p/worlddb/downloads
        /// 
        /// The zipped XML format used here was converted from the above
        /// SQL data source.
        /// </summary>
        /// <param name="pathZipFile"></param>
        /// <param name="countriesFileName"></param>
        /// <param name="regionsFileName"></param>
        /// <param name="citiesFileName"></param>
        /// <returns></returns>
        public static async Task<List<MetaLocationModel>> LoadData(
             string pathZipFile
            , string countriesFileName
            , string regionsFileName
            , string citiesFileName
            )
        {
            return await Task.Run<List<MetaLocationModel>>(async () =>
            {
                 // Load all regions into a dicationary collection
                 Dictionary<string, MetaLocationModel> isoDicRegions = await
                      IsoDicFromZipXml(pathZipFile, regionsFileName);

                 // Load cities (that belong to 1 region) into a dictionary collection
                 List<MetaLocationModel> isoCities
                     = await FromZipXml(pathZipFile, citiesFileName);

                 // Insert each city into its region
                 foreach (var cityItem in isoCities)
                 {
                     int isoCountryLength = cityItem.ISO.IndexOf('-');
                     int isoRegionLength = cityItem.ISO.IndexOf('-', isoCountryLength + 1);
                     string isoRegion = cityItem.ISO.Substring(0, isoRegionLength);

                     MetaLocationModel regionItem;
                     isoDicRegions.TryGetValue(isoRegion, out regionItem);

                     if (regionItem != null)
                     {
                         cityItem.SetParent(regionItem);
                         regionItem.ChildrenAdd(cityItem);
                     }
                 }

                 // Load all countries (about 220 world-wide) into a collection
                 List<MetaLocationModel> isoCountries = null;
                 isoCountries = await FromZipXml(pathZipFile, countriesFileName);

                 // Insert all regions (and cities below them) into their countries
                 foreach (var regionItem in isoDicRegions.Values)
                 {
                     int isoCountryLength = regionItem.ISO.IndexOf('-');
                     string isoCountry = regionItem.ISO.Substring(0, isoCountryLength);

                     var countryItem = isoCountries.Where(x => x.ISO.Equals(isoCountry, StringComparison.InvariantCulture)).FirstOrDefault();

                     if (countryItem != null)
                     {
                         regionItem.SetParent(countryItem);
                         countryItem.ChildrenAdd(regionItem);
                     }
                 }

                 return isoCountries;
             });
        }

        #region Xml Reader Methods
        /// <summary>
        /// Reads all Meta Location Models directly from a given Xml file.
        /// </summary>
        /// <param name="targetFileName">Path the Xml file eg.: @".\Resources\countries.xml"</param>
        /// <returns></returns>
        public static Task<List<MetaLocationModel>> ReadFromXml(string targetFileName)
        {
            return Task.Run(() =>
            {
                List<MetaLocationModel> list = null;

                try
                {
                    using (StreamReader sr = new StreamReader(targetFileName, Encoding.UTF8))
                    {
                        using (TextReader reader = TextReader.Synchronized(sr))
                        {
                            object ds = new XmlSerializer(typeof(List<MetaLocationModel>)).Deserialize(reader);

                            list = ds as List<MetaLocationModel>;
                        }
                    }
                }
                catch (System.Exception)
                {
                    throw;
                }

                return list;
            });
        }

        /// <summary>
        /// Reads all Meta Location Models directly from a given Xml file.
        /// </summary>
        /// <param name="zipFileFullPath">@".\Resources\lokasyon.zip"</param>
        /// <param name="targetFileName">Path the Xml file eg.: "countries.xml"</param>
        /// <returns></returns>
        public static Task<List<MetaLocationModel>> FromZipXml(
              string zipFileFullPath
            , string targetFileName)
        {
            return Task.Run(() =>
            {
                List<MetaLocationModel> list = null;

                try
                {
                    var compressedFile = System.IO.Compression.ZipFile.OpenRead(zipFileFullPath)
                        .Entries.Where(x => x.Name.Equals(targetFileName, StringComparison.InvariantCulture))
                        .FirstOrDefault().Open();

                    using (StreamReader sr = new StreamReader(compressedFile, Encoding.UTF8))
                    {
                        using (TextReader reader = TextReader.Synchronized(sr))
                        {
                            object ds = new XmlSerializer(typeof(List<MetaLocationModel>)).Deserialize(reader);

                            list = ds as List<MetaLocationModel>;
                        }
                    }
                }
                catch (System.Exception)
                {
                    throw;
                }

                return list;
            });
        }

        /// <summary>
        /// Reads all Meta Location Models directly from a Zipped Xml file.
        /// </summary>
        /// <param name="zipFileFullPath"></param>
        /// <param name="targetFileName"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, MetaLocationModel>> IsoDicFromZipXml(
              string zipFileFullPath
            , string targetFileName)
        {
            List<MetaLocationModel> isoList = await FromZipXml(zipFileFullPath, targetFileName);

            Dictionary<string, MetaLocationModel> isoDictionary = new Dictionary<string, MetaLocationModel>();

            foreach (var item in isoList)
                isoDictionary.Add(item.ISO, item);

            return isoDictionary;
        }
        #endregion Xml Reader Methods
    }
}
