/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Holds the minor objects for the data entries into the BTree and B+Tree structures.
*/
namespace NodeData{
  public abstract class Data{

  }

  public class Person : Data{
    private string _Name;

    public Person(string name){
      _Name = name;
    }

    public string Name{
      get { return _Name; }
      set { _Name = value; }
    }

    public override string ToString()
    {
      return "{\"name\":\"" + _Name + "\"}";
    }
  }
}