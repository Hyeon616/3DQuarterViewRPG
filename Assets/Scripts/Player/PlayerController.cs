using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    private Vector2 inputVec;

    void Update()
    {
        transform.position += new Vector3(inputVec.x, 0, inputVec.y) * moveSpeed * Time.deltaTime;
    }

    public void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }
}
