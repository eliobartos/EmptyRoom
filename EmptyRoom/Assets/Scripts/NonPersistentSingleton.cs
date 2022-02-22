using System.Collections;
using UnityEngine;

public class NonPersistentSingleton<T> : MonoBehaviour
where T : MonoBehaviour {
    // The single instance of this class, it is private so that we control it only from this class
    private static T _instance { get; set; }

    // Public get so that anyone can get this one instance
    public static T instance {
        get {
            return _instance;
        }
    }

    protected virtual void Awake() {
        if (_instance == null) {

            // Set up the first instance
            // Need to call base.Awake() from the awake function of child class, needs to be protected override void Awake() methond implemented in child
            _instance = FindObjectOfType<T>();

        } else {
            // Destory any new instances of this class
            // other stuff for singleton method should be called in start, because awake can be called for multiple instances
            Destroy(gameObject);
        }
    }
        
}
