using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public  class SoDataRow
{
   [Header("Id要大于0才有效")]
   public int id;
   public string comment;
   
}

public class SoDataTable<T> : SoDataTableBase  where T:SoDataRow
{
   public List<T> soDataRows = new List<T>();
   public SoDataRow GetDataRow(int id)
   {
      foreach (var soDataRow in soDataRows)
      {
         if (soDataRow.id == id)
         {
            return soDataRow;
         }
      }

      return null;
   }
}
