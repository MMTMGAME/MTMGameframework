using System;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    public float checkDistance = 1.0f; // 检测地面的距离
    private LayerMask groundLayer; // 地面层
    public bool grounded; // 是否接触地面
    public Action<bool> onGroundedStatusChange; // 接触地面状态变化时的回调


   
    private void Awake()
    {
        // 设置地面图层
        groundLayer = LayerMask.GetMask("Scene"); // 确保这里的 "Ground" 是你的地面图层的名称
    }

    private void FixedUpdate()
    {
        bool wasGrounded = grounded;
        grounded = CheckGrounded();

        if (wasGrounded != grounded)
        {
            onGroundedStatusChange?.Invoke(grounded);
        }
    }

    private bool CheckGrounded()
    {
        // 向下射线检测
        RaycastHit hit;
        bool hitGround = Physics.Raycast(transform.position+transform.up*0.2f, Vector3.down, out hit, checkDistance, groundLayer);
        return hitGround;
    }

    public float GetDistanceToGround()
    {
        // 声明一个变量来存储离地面的距离
        float distanceToGround = float.MaxValue;

        // 向下射线检测
        RaycastHit hit;
        bool hitGround = Physics.Raycast(transform.position + transform.up * 0.2f, Vector3.down, out hit, 999, groundLayer);

        if (hitGround)
        {
            // 如果检测到地面，获取离地面的距离
            distanceToGround = hit.distance;
        }

        // 返回检测结果和距离
        return  distanceToGround;

    }

    // 可视化射线检测
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // 设置 Gizmos 颜色为红色
        Vector3 start = transform.position+transform.up*0.2f; // 射线的起点
        Vector3 end = start + Vector3.down * checkDistance; // 射线的终点

        //Gizmos.DrawLine(start, end); // 绘制一条线
    }
}