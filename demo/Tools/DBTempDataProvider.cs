using System.Collections.Generic;
using demo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class DBTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context)
    {
        // do bazy ewentualnie mozna dodac id sesji jesli jest taka potrzeba
        var tempDataDictionary = new Dictionary<string, object>();
        using (var db = new CacheDbContext())
        {
            var entries = db.TempData;
            foreach (var entry in entries) {
                tempDataDictionary.Add(entry.Key, entry.Value);
                db.Remove(entry);
            }
        }
        return tempDataDictionary;
    }

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        using (var db = new CacheDbContext())
        {
            foreach (var item in values)
            {
                 var myEntity = db.TempData.Find(item.Key);
       
                 if (myEntity == null) {
                     db.Add(new DBTempData{Key = item.Key, Value=item.Value.ToString()});
                }
                else {
                    myEntity.Value = item.Value.ToString();
                }
        
            }
            db.SaveChanges();
        }
    }
}
