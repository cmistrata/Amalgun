using UnityEngine;

public class LimitFramerate : MonoBehaviour {
    public int Target = 60;
    void Start() {
        Application.targetFrameRate = Target;
    }
}
