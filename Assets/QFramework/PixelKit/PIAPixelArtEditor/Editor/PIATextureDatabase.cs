using UnityEngine;
using System.Linq;

// easy way to cache all the needed editor textures
public class PIATextureDatabase{
    private Texture2D[] database;

    private static PIATextureDatabase _instance;
    public static PIATextureDatabase Instance {
        get {
            if (_instance == null)
                _instance = new PIATextureDatabase();
            return _instance;
        }
        set {
            _instance = value;
        }
    }
    public PIATextureDatabase() {
        database = Resources.LoadAll<Texture2D>("TextureDatabase");
    }
    public Texture2D GetTexture(string name) {
        Texture2D found = database.Where((x) => x.name == name).FirstOrDefault();
        if (found == null)
            found = new Texture2D(2, 2);
        return found;
    }


}
