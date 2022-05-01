using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    Rigidbody rb;
    RectTransform rect;
    public Transform joystick, cam;
    public float speed;

    Animator anim;

    public bool useAnim, rotate;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rect = joystick.GetComponent<RectTransform>();
        if (useAnim) anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 pos = Vector3.right * (joystick.localPosition.x / rect.sizeDelta.x * speed) + Vector3.forward * (joystick.localPosition.y / rect.sizeDelta.x * speed);
        Vector3 newPos = new Vector3(pos.x, rb.velocity.y, pos.z);
        rb.velocity = newPos; 
        cam.position = Vector3.Lerp(cam.position, transform.position + new Vector3(0, 5f, 0), 0.5f);

        if (rotate) transform.LookAt(transform.position + (Vector3.forward * joystick.localPosition.y / rect.sizeDelta.y) + (Vector3.right * joystick.localPosition.x / rect.sizeDelta.x));
        
        if (!useAnim) return;
        if (joystick.localPosition != Vector3.zero)
        {
            anim.SetBool("walk", true);
            anim.SetFloat("walkSpeed", Vector3.Distance(joystick.localPosition, Vector3.zero) / rect.sizeDelta.x);
        }
        else anim.SetBool("walk", false);
    }
}