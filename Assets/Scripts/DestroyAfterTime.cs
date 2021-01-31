using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float TimeUntilDestroy;

	void Awake ()
    {
        Destroy(gameObject, TimeUntilDestroy);
	}
}
