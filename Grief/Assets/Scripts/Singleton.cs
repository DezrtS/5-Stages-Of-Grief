using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Creates a private static generic variable for the instance of the singleton
    private static T instance;

    // Creates a static generic field to retrieve the private instance variable
    public static T Instance
    {
        get
        {   
            // Checks to see if the instance variable is not defined,
            // Checks to see if its class is in the scene, and if not,
            // A new instance of the specified class is created
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    GameObject singletonObj = new GameObject();
                    singletonObj.name = typeof(T).ToString();
                    instance = singletonObj.AddComponent<T>();
                }
            }

            return instance;
        }
    }

    public virtual void Awake()
    {
        // Destroys the current instance that runs this Awake() function if there is another instance of this class in the scene
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = GetComponent<T>();

        //DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            return;
        }
    }
}

