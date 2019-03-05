using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class HybridECSInstantiator : MonoBehaviour
{

    [Header("General Setting:")]
    [SerializeField]
    float universeRadius;

    [Header("Sun:")]
    [SerializeField]
    GameObject sunPrefab;
    [SerializeField]
    Vector3 sunPosition;

    [Header("Moon:")]
    [SerializeField]
    GameObject moonPrefab;
    [SerializeField]
    float minMoonMovementSpeed;
    [SerializeField]
    float maxMoonMovementSpeed;

    [Header("Stars:")]
    [SerializeField]
    GameObject starPrefab;
    [SerializeField]
    float minStarsize;
    [SerializeField]
    float maxStarsize;
    [SerializeField]
    int starsAmount;
    [SerializeField]
    [Range(0, 100)]
    float minTwinkleFrequency;
    [SerializeField]
    [Range(0, 100)]
    float maxTwinkleFrequency;

    /// <summary>
    /// 椭圆轨道模拟点 点越多 轨道越精确
    /// </summary>
    [Header("Orbital Elipses:")]
    [SerializeField]
    int elipseSegments;
    /// <summary>
    /// 椭圆轨道的宽度
    /// </summary>
    [SerializeField]
    float elipseWidth;
    [SerializeField]
    GameObject orbitalElipsePrefab;

    /// <summary>
    /// 行星数量
    /// </summary>
    [Range(1, 20)]
    public int planetCount = 10;

    [Header("Planets:")]
    [SerializeField]
    List<Planet> planets = new List<Planet>();

    static HybridECSInstantiator instance;
    public static HybridECSInstantiator Instance { get { return instance; } }

    GameObject sun;

    void Awake()
    {
        instance = this;
        PlaceSun();
        PlaceStars();
        PlacePlanets();
    }

    Transform CreateParentTransform(string name)
    {
        var go = new GameObject(name);
        return go.transform;
    }

    void PlaceSun()
    {
        sun = Instantiate(sunPrefab, sunPosition, Quaternion.identity);
        sun.transform.parent = CreateParentTransform("Sun");
    }

    void PlaceStars()
    {
        var starParent = CreateParentTransform("Stars");

        for (var i = 0; i < starsAmount; i++)
        {
            var star = Instantiate(starPrefab);

            var starTrans = star.transform;
            starTrans.parent = starParent;
            var starComponent = star.GetComponent<StarComponent>();
            starComponent.twinkleFrequency = Random.Range(minTwinkleFrequency, maxTwinkleFrequency);

            var randomStarScale = Random.Range(minStarsize, maxStarsize);
            starTrans.localScale = Vector3.one * randomStarScale;
            starTrans.position = Random.onUnitSphere * universeRadius;
            star.SetActive(true);
        }
    }

    /// <summary>
    /// 绘制银河系椭圆
    /// </summary>
    /// <param name="line"></param>
    /// <param name="ellipse"></param>
    void DrawOrbitalElipse(LineRenderer line, OrbitalEllipse ellipse)
    {
        var drawPoints = new Vector3[elipseSegments + 1];

        for (var i = 0; i < elipseSegments; i++)
        {
            drawPoints[i] = ellipse.Evaluate(i / (elipseSegments - 1));
        }
        drawPoints[elipseSegments] = drawPoints[0];

        line.useWorldSpace = false;
        line.positionCount = elipseSegments + 1;
        line.startWidth = elipseWidth;
        line.SetPositions(drawPoints);
    }

    void PlacePlanets()
    {
        var planetParent = CreateParentTransform("Planets");

        for (var i = 0; i < planets.Count; i++)
        {
            var planetData = planets[i];
            var planet = Instantiate(planetData.planetPrefab);
            planet.transform.parent = planetParent;

            var planetComponent = planet.GetComponent<PlanetComponent>();
            planetComponent.rotationSpeed = planetData.rotationSpeed;
            planetComponent.orbitDuration = planetData.orbitDuration;
            planetComponent.orbit = planetData.orbit;

            var elipse = Instantiate(orbitalElipsePrefab, sunPosition, Quaternion.identity);
            elipse.transform.parent = sun.transform;
            DrawOrbitalElipse(elipse.GetComponent<LineRenderer>(), planetData.orbit);

            if (planetData.hasMoon)
            {
                GenerateMoon(planet);
            }
        }
    }

    void GenerateMoon(GameObject planet)
    {
        var moonParent = CreateParentTransform("Moons");

        var moon = Instantiate(moonPrefab);
        moon.transform.parent = moonParent;

        var moonComponent = moon.GetComponent<MoonComponent>();
        moonComponent.movementSpeed = Random.Range(minMoonMovementSpeed, maxMoonMovementSpeed);
        moonComponent.parentPlanet = planet;
    }

    /// <summary>
    /// 先保证有一个数据
    /// </summary>

    [ContextMenu("序列化行星信息")]
    void GeneratePlanets()
    {

        if (planets.Count < 1 || planets[0].planetPrefab == null)
        {
            Debug.LogError("请保证有一个行星数据并且有prefab依赖信息");
            return;
        }

        var templatePrefab = planets[0].planetPrefab;
        planets.Clear();
        for (var i = 0; i < planetCount; i++)
        {

            var planet = new Planet();
            planet.planetPrefab = templatePrefab;

            var orbit = new OrbitalEllipse();
            orbit.xExtent = Random.Range(30, 50);
            orbit.yExtent = Random.Range(30, 50);
            orbit.tilt = Random.Range(0, 70);

            planet.orbit = orbit;
            planet.hasMoon = Random.Range(1, 100) > 50;
            planet.rotationSpeed = Random.Range(10, 100);
            planet.orbitDuration = Random.Range(3, 20);

            planets.Add(planet);
        }
    }
}
/// <summary>
/// 椭圆生成器
/// </summary>
[Serializable]
public class OrbitalEllipse
{
    public float xExtent;
    public float yExtent;
    /// <summary>
    /// 倾斜
    /// </summary>
    public float tilt;

    /// <summary>
    /// 根据时间计算椭圆生成路径 移动轨迹
    /// </summary>
    /// <param name="_t"></param>
    /// <returns></returns>
    public Vector3 Evaluate(float _t)
    {
        var up = new Vector3(0f, Mathf.Cos(tilt * Mathf.Deg2Rad), -Mathf.Sin(tilt * Mathf.Deg2Rad));
        var angle = Mathf.Deg2Rad * 360f * _t;

        var x = Mathf.Sin(angle) * xExtent;
        var y = Mathf.Cos(angle) * yExtent;

        return up * y + Vector3.right * x;
    }
}

[Serializable]
public class Planet
{
    public GameObject planetPrefab;
    /// <summary>
    /// orbit 运行轨道
    /// </summary>
    public OrbitalEllipse orbit;
    public bool hasMoon;

    [Header("Movement Settings:")]
    public float rotationSpeed;
    public float orbitDuration;
}

