
namespace NodeData{
  public abstract class Data{

  }

  public class Person : Data{
    private string _Name;

    public Person(string name){
      _Name = name;
    }

    public string name{
      get { return _Name; }
      set { _Name = value; }
    }
  }
}