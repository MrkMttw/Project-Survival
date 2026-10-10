
using UnityEngine;

public class FireflyBehaviour : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 0.5f;
    public float directionChangeInterval = 2f;
    public float wanderRadius = 2f;

    [Header("Glow")]
    public Transform glow;
    public float pulseSpeed = 2f;
    public float minGlowScale = 0.7f;
    public float maxGlowScale = 1.2f;

    private Vector2 randomDirection;
    private Vector2 spawnPosition;
    private float directionTimer;
    private float pulseOffset;

    private void Start()
    {
        spawnPosition = transform.position;
        pulseOffset = Random.Range(0f, 10f);
        ChangeDirection();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateGlow();
    }

    private void UpdateMovement()
    {
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0f)
        {
            ChangeDirection();
        }

        Vector2 currentPosition = transform.position;
        Vector2 nextPosition = currentPosition +
            randomDirection * moveSpeed * Time.deltaTime;

        if (Vector2.Distance(nextPosition, spawnPosition) > wanderRadius)
        {
            Vector2 directionToSpawn =
                (spawnPosition - currentPosition).normalized;

            randomDirection = Vector2.Lerp(
                randomDirection,
                directionToSpawn,
                0.5f
            ).normalized;

            nextPosition = currentPosition +
                randomDirection * moveSpeed * Time.deltaTime;
        }

        transform.position = nextPosition;
    }

    private void ChangeDirection()
    {
        randomDirection = Random.insideUnitCircle.normalized;
        directionTimer = directionChangeInterval *
                         Random.Range(0.5f, 1.5f);
    }

    private void UpdateGlow()
    {
        if (glow == null)
        {
            return;
        }

        float pulse =
            (Mathf.Sin((Time.time + pulseOffset) * pulseSpeed) + 1f) / 2f;

        float scale = Mathf.Lerp(
            minGlowScale,
            maxGlowScale,
            pulse
        );

        glow.localScale = Vector3.one * scale;
    }
}

/*
# FireflyBehaviour

## Overview
`FireflyBehaviour` controls the movement and glowing effect of individual fireflies in the world. Each firefly wanders randomly around its original spawn position while its glow continuously pulses.

## Features
- **Random Movement:** Moves in random directions and periodically changes direction.
- **Wander Radius:** Keeps the firefly near its original spawn position.
- **Return Direction:** Adjusts its movement toward its spawn position when it wanders too far away.
- **Pulsing Glow:** Smoothly changes the scale of the Glow object to simulate a flickering light effect.
- **Independent Animation:** Each firefly has a randomized glow animation offset.

## Inspector Settings

### Movement
| Variable | Description | Default |
|---|---|---:|
| `moveSpeed` | Movement speed of the firefly. | 0.5 |
| `directionChangeInterval` | Base time interval before choosing a new movement direction. | 2 |
| `wanderRadius` | Maximum intended distance from the original spawn position. | 2 |

### Glow
| Variable | Description | Default |
|---|---|---:|
| `glow` | Transform of the child Glow object. | None |
| `pulseSpeed` | Speed of the glow pulsing animation. | 2 |
| `minGlowScale` | Minimum scale of the glow during pulsing. | 0.7 |
| `maxGlowScale` | Maximum scale of the glow during pulsing. | 1.2 |

## Setup
1. Attach `FireflyBehaviour` to the root of the Firefly prefab.
2. Create or use a child object named `Glow`.
3. Assign the child's Transform to the `Glow` field in the Inspector.
4. Configure the movement and glow settings as needed.
5. Use the Firefly prefab with the world's spawning system.

## How It Works
When the firefly starts, it records its initial world position and randomizes its glow animation offset. It then moves in a randomly selected direction, periodically changing direction.

When it moves beyond its configured wander radius, it adjusts its direction toward its original spawn position. The Glow child continuously scales between its minimum and maximum values using a sine wave.

## Notes
- Movement uses world coordinates, so the script does not require a Canvas or player reference.
- The wander radius is a steering limit rather than a strict boundary; the firefly may move slightly beyond it while turning back.
- The Glow field must reference the correct child Transform for the pulsing effect to work.
- Adjust `moveSpeed`, `directionChangeInterval`, and `wanderRadius` together to achieve natural-looking movement.
*/