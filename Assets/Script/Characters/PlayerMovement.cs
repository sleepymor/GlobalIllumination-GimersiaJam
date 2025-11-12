using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public SpriteRenderer spriteRenderer; // untuk flip sprite
    public float speed = 5f;
    public float jumpPower = 5f;
    public float gravity = 9.81f;
    private Vector3 moveDirection;
    private bool canMove = true;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Flip sprite berdasarkan arah horizontal
        if (horizontal > 0)
            spriteRenderer.flipX = false;
        else if (horizontal < 0)
            spriteRenderer.flipX = true;

        // Movement
        if (direction.magnitude >= 0.1f)
        {
            // Hanya gerakkan karakter ke arah input, tanpa rotasi 3D
            Vector3 move = direction * speed * Time.deltaTime;
            controller.Move(move);
        }

        // Jump & Gravity
        if (controller.isGrounded)
        {
            moveDirection.y = 0f;

            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpPower;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }
}
