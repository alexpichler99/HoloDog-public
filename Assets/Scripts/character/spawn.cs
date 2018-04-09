using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawn : MonoBehaviour {

	public Rigidbody rb;

	// Use this for initialization
	void Start () {
        StartCoroutine(Example());
        rb.useGravity = true;
    }

    IEnumerator Example()
    {
        print(Time.time);
        yield return new WaitForSeconds(2);
        print(Time.time);
    }
    // Update is called once per frame
    void Update () {
	}
}
