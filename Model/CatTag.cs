using System;
using System.ComponentModel.DataAnnotations.Schema;

public class CatTag
{
    public string CatId { get; set; }  // References the CatEntity Id
    public int TagId { get; set; }  // References the TagEntity Id
}
