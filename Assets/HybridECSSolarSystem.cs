using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

/// <summary>
/// 行为处理
/// 各个struct是对数据的进一步封装提取 高度集中管理
/// </summary>
public class HybridECSSolarSystem : ComponentSystem
{

    struct Stars
    {
        public StarComponent starComponent;
        public MeshRenderer renderer;
    }

    struct Planets
    {
        public PlanetComponent planetComponent;
        public Transform transform;
    }

    struct Moons
    {
        public Transform transform;
        public MoonComponent moonComponent;
    }

    protected override void OnUpdate()
    {
        var starEntities = GetEntities<Stars>();
        foreach (var starEntity in starEntities)
        {
            int timeAsInt = (int)Time.time;

            if (Random.Range(1f, 1000f) < starEntity.starComponent.twinkleFrequency)
            {
                starEntity.renderer.enabled = timeAsInt % 2 == 0;
            }
            //Debug.Log("------");
            //starEntity.renderer.enabled = Random.Range(1f, 100f) % starEntity.starComponent.twinkleFrequency == 0;
        }

        var planetEntities = GetEntities<Planets>();
        foreach (var planetEntity in planetEntities)
        {

            planetEntity.transform.Rotate(Vector3.up * Time.deltaTime * planetEntity.planetComponent.rotationSpeed, Space.Self);
            planetEntity.transform.position = planetEntity.planetComponent.orbit.Evaluate(Time.time / planetEntity.planetComponent.orbitDuration);
        }

        var moonEntities = GetEntities<Moons>();
        foreach (var moonEntity in moonEntities)
        {
            Vector3 parentPos = moonEntity.moonComponent.parentPlanet.transform.position;
            Vector3 desiredPos = (moonEntity.transform.position - parentPos).normalized * 5f + parentPos;

            moonEntity.transform.position = Vector3.MoveTowards(moonEntity.transform.position, desiredPos, moonEntity.moonComponent.movementSpeed);
            moonEntity.transform.RotateAround(moonEntity.moonComponent.parentPlanet.transform.position, Vector3.up, moonEntity.moonComponent.movementSpeed);
        }

    }
}
