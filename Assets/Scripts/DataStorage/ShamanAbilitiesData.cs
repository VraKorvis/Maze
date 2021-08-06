using System;

namespace Maze.Data.Storages {

    [Serializable]
    public abstract class ShamanAbilitiesData {
        public abstract int     GetId();
        public abstract string  GetDescription();
        public abstract int[]   GetCost();
        public abstract int[]   GetRank();
        public abstract float[] GetCooldown();
    }

    [Serializable]
    public class FearData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;
        public int[]   throughObstacle;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class FrostNovaData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;
        public int[]   throughObstacle;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class SlowdownData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;
        public float[] influence;
        public int[]   throughObstacle;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }

    }

    [Serializable]
    public class JumpData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class BlinkAutoData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class InvulnerabilityData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class HeroLightsData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class SonarData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class ShowRagData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

    [Serializable]
    public class ShowHomeData : ShamanAbilitiesData {
        public int     id;
        public string  description;
        public int[]   cost;
        public int[]   rank;
        public float[] duration;
        public float[] cooldown;
        public float[] radius;

        public override int     GetId()          { return id; }
        public override string  GetDescription() { return description; }
        public override int[]   GetCost()        { return cost; }
        public override int[]   GetRank()        { return rank; }
        public override float[] GetCooldown()    { return cooldown; }
    }

}