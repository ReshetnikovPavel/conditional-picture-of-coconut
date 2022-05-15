using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Bullet;
using Extensions;
using Game;
using GridTools;
using Player;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SearchService;
using Random = UnityEngine.Random;

namespace Assets.Enemies
{
    public class YellowBoss : MonoBehaviour
    {
        private float latestAimAngle;

        public HealthObj Health;
        public ParticleSystem boom;
        [SerializeField] private int maxHealth;
        [SerializeField] protected BulletYellowBoss FireBallPrefab;
        private Rigidbody2D Rb;

        private SpriteRenderer sprite;
        public GameObject healthObjPrefab;

        public GameObject ForceField;

        private Stage currentStage;
        private State state;
        [SerializeField] private AttackStage attackStage;

        public GridObj Grid;
        private PathFinding pathFinder;

        private Vector3 homePosition;
        private float homeRadius;

        private Vector3 startingPosition;
        private Vector3 roamPosition;
        private Vector3 nextTarget;
        private int nextTargetIndex;

        private int countFailSearch;
        private const int countFailSearchLimit = 5;

        private Vector3 direction;

        private List<int2> path;

        private float followingStartTime;
        private const float followingTime = 6f;

        private float reloadBoomStart;
        private float boomReloadTime = 1f;

        private float attackPauseStart;
        private const float attackPauseTime = 2f;
        
        private float pauseStart;
        private const float pauseTime = 1f;

        private float? startWeak;
        private const float weakTime = 5f;

        private float reloadStart;
        //private float reloadTime = 0.3f;
        
        private float targetRange = 25f;
        private float fireRange;
        private float moveSpeed;

        private int boomDamage = 20;

        private Queue<AttackStage> attackStrategy;
        public Bomber BomberPrefab;
        private List<Bomber> bombers;

        private int maxBombers = 20;
        private int maxFireBalls = 100;
        private int currentFireBalls = 0;
        private Vector3 dashTarget;

        private enum Stage
        {
            None,
            SearchingPath,
            Moving,
            Pause
        }

        private enum State
        {
            Roaming
        }

        private enum AttackStage
        {
            None,
            Pause,
            SpawnBombers,
            FireBalls,
            Weak,
            Dash
        }

        private void Start()
        {
            pathFinder = new PathFinding();
            Health = Instantiate(healthObjPrefab, transform).GetComponent<HealthObj>();
            Health.maxHealthPoints = maxHealth;
            Health.IsImmortal = true;

            sprite = GetComponent<SpriteRenderer>();
            Rb = GetComponent<Rigidbody2D>();
            fireRange = Rb.transform.localScale.x * 2f;

            homePosition = transform.position;
            startingPosition = transform.position;
            UpdateTarget(GetRandomPosition());

            homeRadius = 25;

            currentStage = Stage.None;
            attackStage = AttackStage.None;
            moveSpeed = 6f;
            followingStartTime = Time.time;
            reloadBoomStart = Time.time;
            startWeak = null;

            attackStrategy = new Queue<AttackStage>(
                new[]
                {
                    AttackStage.SpawnBombers,
                    AttackStage.FireBalls,
                    AttackStage.Dash,
                    AttackStage.Dash,
                    AttackStage.Dash,
                    AttackStage.FireBalls,
                    AttackStage.Weak
                }
            );

        }

        private void FixedUpdate()
        {
            if (Health.CurrentHealthPoints <= 0)
                Die();


            var target = IsNearToPlayer(targetRange) 
                ? GameData.player.transform.position 
                : nextTarget;
            
            UpdateAimFire(target);
            UpdateEyeDirection(target);

            if (IsNearToPlayer(fireRange))
                Fire(boomReloadTime);

            switch (attackStage)
            {
                case AttackStage.None:
                    if (IsNearToPlayer(targetRange))
                    {
                        EndStage();
                    }
                    else
                        sprite.color = Color.white;
                    break;
                case AttackStage.Pause:
                    var difference = Time.time - attackPauseStart;
                    if (difference >= attackPauseTime)
                        UpdateAttackStage();
                    break;
                case AttackStage.SpawnBombers:
                    SpawnBombers();
                    break;
                case AttackStage.FireBalls:
                    CreateFireBalls();
                    break;
                case AttackStage.Weak:
                    BeWeak();
                    break;
                case AttackStage.Dash:
                    dashTarget = dashTarget == Vector3.zero ? GameData.player.transform.position : dashTarget;
                    DoDash(dashTarget);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (state)
            {
                case State.Roaming:
                    if (countFailSearch > 0)
                        UpdateTarget(countFailSearch >= countFailSearchLimit
                            ? homePosition
                            : GetRandomPosition());
                    Move(roamPosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Move(Vector3 target)
        {
            switch (currentStage)
            {
                case Stage.None:
                    currentStage = Stage.SearchingPath;
                    StartSearchNextTarget(target);
                    break;
                case Stage.SearchingPath:
                    break;
                case Stage.Moving:
                    MoveToNextTarget();
                    break;
                case Stage.Pause:
                    var difference = Time.time - pauseStart;
                    if (difference >= pauseTime)
                    {
                        UpdateTarget(GetRandomPosition());
                        currentStage = Stage.None;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MoveWithTimer(Vector3 target, float timeFollow)
        {
            var timeFollowing = Time.time - followingStartTime;
            if (timeFollowing >= timeFollow && currentStage != Stage.SearchingPath)
            {
                currentStage = Stage.None;
                followingStartTime = Time.time;
                return;
            }

            Move(target);
        }

        private Task<List<int2>> FindPath(int2 startGridPosition, int2 endGridPosition, int maxDeep)
        {
            var task = new Task<List<int2>>(() =>
                pathFinder.FindPathAStar(Grid.Grid, startGridPosition, endGridPosition, maxDeep));

            task.Start();
            return task;
        }

        private async void StartSearchNextTarget(Vector3 target)
        {
            UpdateTarget(target);

            var startGridPosition = Grid.WorldToGridPosition(startingPosition);
            var endGridPosition = Grid.WorldToGridPosition(roamPosition);

            var maxDeep = (int)homeRadius;
            var originalPath = await FindPath(startGridPosition, endGridPosition, maxDeep);

            if (originalPath is null)
            {
                currentStage = Stage.None;
                countFailSearch++;
            }
            else
            {
                countFailSearch = 0;
                path = PathFinding.GetClearPath(originalPath);
                Grid.AddPathsToDraw(path);

                nextTargetIndex = 0;
                UpdateNextTarget();
            }
        }

        private void MoveToNextTarget()
        {
            UpdateDirection(nextTarget);
            var distanceToNextTarget = transform.position.DistanceTo(nextTarget);

            Rb.velocity = direction * moveSpeed;

            if (distanceToNextTarget >= moveSpeed * Time.fixedDeltaTime)
                return;

            if (nextTargetIndex == path.Count - 1)
            {
                Rb.velocity = Vector2.zero;
                currentStage = Stage.Pause;
                pauseStart = Time.time;
                return;
            }

            UpdateNextTarget();
        }

        private void UpdateNextTarget()
        {
            for (var i = path.Count - 1; i > nextTargetIndex; i--)
            {
                var target = Grid.GridToWorldPosition(path[i]).ToVector3() + new Vector3(Grid.Grid.CellSize, Grid.Grid.CellSize) / 2;
                var currentPosition = transform.position;
                var distance = currentPosition.DistanceTo(target);
                var currentDirection = (target - currentPosition).normalized;

                var ray = Physics2D.CircleCast(currentPosition.ToVector2(), transform.localScale.y, currentDirection.ToVector2(), distance, Grid.WallsLayerMask);

                if (ray.collider != null)
                    continue;

                nextTargetIndex = i;
                nextTarget = target;
                currentStage = Stage.Moving;
                return;
            }

            nextTargetIndex++;
            nextTarget = Grid.GridToWorldPosition(path[nextTargetIndex]).ToVector3() + new Vector3(Grid.Grid.CellSize, Grid.Grid.CellSize) / 2;
            currentStage = Stage.Moving;
        }

        private void UpdateTarget(Vector3 target)
        {
            startingPosition = transform.position;
            roamPosition = homePosition.DistanceTo(target) > homeRadius
                ? homePosition
                : target;
        }

        private void UpdateDirection(Vector3 target)
        {
            direction = (nextTarget - transform.position).normalized;
        }

        private Vector3 GetRandomPosition()
            => homePosition + Tools.GetRandomDir() * Random.Range(10f, 15f);

        private void UpdateEyeDirection(Vector3 target)
        {
            sprite.flipX = (int)Mathf.Sign(target.x - transform.position.x) == 1;
        }

        private void Fire(float reloadTime)
        {
            var difference = Time.time - reloadBoomStart;
            if (difference < reloadTime)
                return;
            reloadBoomStart = Time.time;

            var objectsToGetDamage = Physics2D.OverlapCircleAll(transform.position, fireRange);

            for (var angle = 0f; angle < Mathf.PI * 2; angle += Mathf.PI / 3)
            {
                Instantiate(boom, transform.position + new Vector3(fireRange * Mathf.Cos(angle), fireRange * Mathf.Sin(angle)), Quaternion.identity);
            }

            foreach (var obj in objectsToGetDamage)
            {
                if (!obj.GetComponentInChildren<Character>())
                    continue;

                var healthObj = obj.GetComponentInChildren<HealthObj>();
                //obj.GetComponent<Rigidbody2D>().AddForce((obj.transform.position - transform.position).normalized * 10, ForceMode2D.Impulse);
                if (healthObj != null)
                    healthObj.Damage(boomDamage);
            }
        }

        private bool CheckReload(float reloadTime)
        {
            var difference = Time.time - reloadStart;
            if (difference < reloadTime)
                return false;
            reloadStart = Time.time;
            return true;
        }

        private void SpawnBombers()
        {
            const float reloadTime = 0.2f;
            if (!CheckReload(reloadTime))
                return;
            
            bombers ??= new List<Bomber>();

            if (bombers.Count < maxBombers)
            {
                var dir = (GameData.player.transform.position - transform.position).normalized;
                var bomber = Instantiate(BomberPrefab, transform.position + dir,
                    Quaternion.identity);
                bomber.GetComponent<Rigidbody2D>().AddForce(dir * 20f, ForceMode2D.Impulse);
                bomber.Grid = Grid;
                bombers.Add(bomber);
            }
               
            else if(bombers.All(obj => obj == null))
            {
                bombers.Clear();
                EndStage();
            }
        }

        private void CreateFireBalls()
        {
            const float reloadTime = 0.05f;
            if (!CheckReload(reloadTime))
                return;

            currentFireBalls++;
            if (currentFireBalls > maxFireBalls)
            {
                currentFireBalls = 0;
                EndStage();
            }

            var direction = (GameData.player.transform.position - transform.position).normalized 
                            + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            var fireball = Instantiate(
                FireBallPrefab, 
                transform.position + direction * transform.localScale.x * 1.5f, 
                Quaternion.identity);
            fireball.GetComponent<Rigidbody2D>().AddForce(direction * 17, ForceMode2D.Impulse);
        }

        private void DoDash(Vector2 target)
        {
            const float reloadTime = 0.1f;
            Fire(reloadTime);

            var dir = target - transform.position.ToVector2();
            Rb.AddForce(dir / 2, ForceMode2D.Impulse);

            if (transform.position.DistanceTo(target) < fireRange)
            {
                Fire(0);
                dashTarget = Vector3.zero;
                currentStage = Stage.Pause;
                pauseStart = Time.time;
                EndStage();
            }
                
        }

        private void BeWeak()
        {
            if (!startWeak.HasValue)
            {
                ForceField.SetActive(false);
                startWeak = Time.time;
            }
                
            Health.IsImmortal = false;
            var difference = Time.time - startWeak.Value;
            if (difference < weakTime)
                return;
            Health.IsImmortal = true;
            startWeak = null;
            ForceField.SetActive(true);
            EndStage();
        }

        private void Die()
        {
            Destroy(gameObject);
        }

        private bool IsNearToPlayer(float distance)
        {
            try
            {
                return Vector3.Distance(transform.position, GameData.player.transform.position) < distance;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateAimFire(Vector3 target)
        {
            //if (Weapon == null)
            //    return;
            //directionFire = (target - transform.position).normalized;
            //var aimAngle = Mathf.Atan2(directionFire.y, directionFire.x) * Mathf.Rad2Deg - 90f;
            //Weapon.weaponPrefab.transform.RotateAround(Rb.position, Vector3.forward, aimAngle - latestAimAngle);
            //latestAimAngle = aimAngle;
        }

        private void EndStage()
        {
            attackPauseStart = Time.time;
            attackStage = AttackStage.Pause;
        }
        
        private void UpdateAttackStage()
        {
            var nextStage = attackStrategy.Dequeue();
            attackStrategy.Enqueue(nextStage);
            attackStage = nextStage;
        }

        private void ChooseBehaviour()
        {
            if (homePosition.DistanceTo(transform.position) > homeRadius)
                UpdateTarget(homePosition);

            state = State.Roaming;

            //if (IsNearToPlayer(targetRange))
            //{
            //    if (IsNearToPlayer(RunRange))
            //    {
            //        if (state != State.RunFromPlayer)
            //        {
            //            followingStartTime = int.MinValue;
            //            ClearTransformation();
            //        }
            //        state = State.RunFromPlayer;
            //    }
            //    else
            //    {
            //        if (state != State.RunToPlayer)
            //            GetTransformation();
            //        state = State.RunToPlayer;
            //    }

            //}
            //else
            //{
            //    state = State.Roaming;
            //    if (state != State.Roaming)
            //        ClearTransformation();
            //}

        }
    }
}
