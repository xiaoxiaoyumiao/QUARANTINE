using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum VarType
{
    HEALTHY_POP,
    INFECTED_POP_CURR_GEN,
    INFECTED_POP_NEXT_GEN,
    MATERIAL,
    NONE
}

public enum BlockType
{
    ERROR,
    FACTORY,
    HOUSING,
    HOSPITAL,
    QUARANTINE
}

public class Utility
{
    public static int AdaptedRandomNumber(float ratio, int total)
    {
        int part = (int)(Random.Range(0, ratio) * total);
        return part;
    }

    private static GameObject manager = null;
    private static VirusModel model = null;
    public static GameObject GetManager()
    {
        if (manager == null)
        {
            manager = GameObject.FindWithTag("GameController");
        }
        return manager;
    }
    public static VirusModel GetVirusModel()
    {
        if (model == null)
        {
            if (manager == null)
            {
                GetManager();
            }
            model = manager.GetComponent<VirusModel>();
        }
        return model;
    }
}

/// <summary>
/// Variable is a basic data structure defined to cope with batch iteration.
/// It's basically a piece of data, and can be read and modified as common value type.
/// <example>
///    This example will read data out of a Variable, increase it by 1, and write back
///    <code>
///        Variable<int> p(2, VarType.NONE); // data initialized to 2
///        int a = p.Data; // read out
///        a += 1;
///        p.Data = a; // write back
///    </code>
///    Of course you can simplify the code like this:
///    <code>
///        Variable<int> p(2, VarType.NONE); // data initialized to 2
///        p.Data += 1;
///    </code>
/// </example>
/// But there is some time you have to deal with a batch of variables, and you may want to 
/// iterate on them, new value of each depending on old values of some. You can create a 
/// buffer, calculate new values and store them to the buffer during computation, and 
/// assign the buffer to variables after the computation is completed. But when this kind 
/// of operation becomes frequent, the whole process will be annoying. So how about binding 
/// the buffer directly to the variable? This is how the Variable class work. You can write
/// to its DataBuf field while keeping its Data value unchanged, and call Commmit() to 
/// update the value of Data. Plus, Writing to Data field will cause value in DataBuf to 
/// follow, but it's not recommended to read DataBuf unless you are clear about how this field
/// is handled. Here are some examples:
/// <example>
/// the following code will calculate Fibonacci value when iterated.
///     <code>
///         Variable<int> a(1, VarType.NONE);
///         Variable<int> b(0, VarType.NONE);
///         for (int i=0;i<10;++i){
///             a.DataBuf = a.Data + b.Data;
///             b.DataBuf = a.Data;
///             a.Commit(); b.Commit();
///         }
///     </code>
/// </example>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Variable<T> where T: struct
{
    private T data;
    private T dataBuffer;
    private VarType type;
    private bool dirty;

    public T Data
    {
        get
        {
            return data;
        }
        set
        {
            data = value;
            dataBuffer = value;
            dirty = false;
        }
    }
    public T DataBuf
    {
        get
        {
            return dataBuffer;
        }
        set
        {
            dataBuffer = value;
            dirty = true;
        }
    }


    public float priority;
    public List<Variable<T>> outVariables;
    public static readonly float ZERO = 1e-6f;
    
    public Variable(T mData, VarType mType)
    {
        data = mData;
        type = mType;
        dirty = false;
        priority = 0.0f;
        outVariables = new List<Variable<T>>();
    }

    public bool NeedCommit()
    {
        return dirty;
    }
    public void Commit()
    {
        data = dataBuffer;
        dirty = false;
    }

    public void AddOutVariable(Variable<T> var)
    {
        outVariables.Add(var);
    }

    public virtual void Broadcast(float ratio, float offset = 0.0f) { }
}

public class BroadcastVariableInt : Variable<int>
{
    public BroadcastVariableInt(int mData, VarType mType): base(mData, mType) { }

    /// <summary>
    /// Method used for data exchanging between a variable and those of its vicinity 
    /// (stored in outVariables defined in Variable)
    /// </summary>
    /// <param name="ratio">ratio by which Data of this variable will be distributed.
    /// for example, "ratio=0.5f" means that half the Data will be given out.</param>
    /// <param name="offset"></param>
    public override void Broadcast(float ratio, float offset = 0.0f)
    {
        List<Variable<int>> finalOuts = new List<Variable<int>>();
        float prioritySum = 0.0f;
        foreach (Variable<int> ele in outVariables)
        {
            if (ele.priority < offset) // will be ignored
            {
                continue;
            }
            prioritySum += ele.priority + 1;
            finalOuts.Add(ele);
        }

        if (prioritySum < ZERO) return;

        if (ratio <= ZERO) ratio = 0.0f;
        int delta = (int)(Data * ratio);
        if (Data < delta) delta = Data;
        int outSum = delta;
        foreach (Variable<int> ele in finalOuts)
        {
            int alloc = (int)(delta * ((ele.priority+1) / prioritySum));
            if (outSum < alloc) alloc = outSum;
            ele.DataBuf += alloc;
            outSum -= alloc;
        }
        DataBuf -= delta - outSum;
    }
}

[System.Serializable]
public class InfectedStage
{
    [System.NonSerialized]
    private static InfectedStage[] mStages = null;
    public float reproduction;
    public float deathRate;
    public InfectedStage(float mReproduction = 0, float mDeathRate = 0) {
        reproduction = mReproduction;
        deathRate = mDeathRate;
    }

    public static InfectedStage[] stages{
        get
        {
            if (mStages == null)
            {
                VirusModel model = Utility.GetVirusModel();
                mStages = model.stages;
            }
            return mStages;
        }
    }
}

public class InfectedTimer
{
    List<InfectedStage> stages;
    public readonly int period; // T = 3D
    public int timer;
    public int stagePointer;
    public InfectedTimer(int mPeriod)
    {
        stages = new List<InfectedStage>();
        period = mPeriod;
        // stagePointer = mInitStage % InfectedStage.stages.Length;
        stagePointer = 0;
    }

    public int AddStage(InfectedStage stage)
    {
        stages.Add(stage);
        return 0;
    }

    /* will return 1 if a stage cycle complete */
    public int Tick()
    {
        if (stages.Count == 0)
            return 0;
        timer++;
        if (timer == period)
        {
            timer = 0;
            stagePointer = (stagePointer+1) % stages.Count;
            if (stagePointer == 0)
                return 1;
        }

        return 0;
    }

    public int countdown
    {
        get
        {
            return period - timer;
        }
    }

    public float Reproduction
    {
        get
        {
            return stages[stagePointer].reproduction;
        }
    }

    public float DeathRate
    {
        get
        {
            return stages[stagePointer].deathRate;
        }
    }
    
}


/* BlockTypeParameter
 * important parameters that are strongly (or at least potentially) related to block type
 * though some of them may be temporarily the same for all types of blocks
 */
[System.Serializable]
public class BlockTypeParameter
{
    [System.NonSerialized]
    private static BlockTypeParameter mFactory;
    [System.NonSerialized]
    private static BlockTypeParameter mHousing;
    [System.NonSerialized]
    private static BlockTypeParameter mHospital;
    [System.NonSerialized]
    private static BlockTypeParameter mQuarantine;

    public static BlockTypeParameter factory
    {
        get
        {
            if (mFactory == null)
                mFactory = Utility.GetVirusModel().factory;
            return mFactory;
        }
    }
    public static BlockTypeParameter housing
    {
        get
        {
            if (mHousing == null)
                mHousing = Utility.GetVirusModel().housing;
            return mHousing;
        }
    }
    public static BlockTypeParameter hospital
    {
        get
        {
            if (mHospital == null)
                mHospital = Utility.GetVirusModel().hospital;
            return mHospital;
        }
    }
    public static BlockTypeParameter quarantine
    {
        get
        {
            if (mQuarantine == null)
                mQuarantine = Utility.GetVirusModel().quarantine;
            return mQuarantine;
        }
    }

    [System.NonSerialized]
    public BlockType type;
    
    // reproduction rate factor
    public float R_FACTOR = 2.0f;
    // death rate factor
    public float D_FACTOR = 1.0f;
    // material consuming rate factor
    public float CONSUME_FACTOR = -0.01f;
    // material producing rate factor (FOR FACTORY)
    public float PRODUCE_FACTOR = +1f;
    // tax rate
    public float TAX_RATE = 0.05f;
    // ratio by which healthy population will move out at most
    public float HP_MOVE_RATIO = 0.5f;
    // ratio by which current generation of infected population will move out at most
    public float CIP_MOVE_RATIO = 0.9f;
    // ratio by which next generation of infected population will move out at most
    public float NIP_MOVE_RATIO = 0.6f;
    // ratio by which material will move out at most
    public float M_MOVE_RATIO = 0.5f;


    [System.NonSerialized]
    public int RESOURCE_MIN = 0;
    
    // This is a default constructor, parameters will be overwritten by data set in the inspector.
    public BlockTypeParameter(BlockType mType)
    {
        type = mType;
        if (type == BlockType.HOSPITAL)
        {
            R_FACTOR = 0.5f;
            D_FACTOR = 0.0f;
            CONSUME_FACTOR = -0.1f;
        }
    }

}

public class Block
{
    public BlockTypeParameter parameter;
    public BlockType type
    {
        get
        {
            if (parameter == null)
                return BlockType.ERROR;
            return parameter.type;
        }

        set
        {
            switch (value)
            {
                case BlockType.FACTORY:
                    {
                        parameter = BlockTypeParameter.factory;
                        break;
                    }
                case BlockType.HOUSING:
                    {
                        parameter = BlockTypeParameter.housing;
                        break;
                    }
                case BlockType.HOSPITAL:
                    {
                        parameter = BlockTypeParameter.hospital;
                        break;
                    }
                case BlockType.QUARANTINE:
                    {
                        parameter = BlockTypeParameter.quarantine;
                        break;
                    }
                default:
                    {
                        parameter = null;
                        break;
                    }
            }
        }
    }
    public GameObject blockUI;
    public List<Block> outBlocks; // Blocks

    /* material related */

    public bool isWorking = false;
    public float MCRate
    {
        get
        {
            if (isWorking) return parameter.PRODUCE_FACTOR;
            else return parameter.CONSUME_FACTOR;
        }

    }

    public int RESOURCE_MIN
    {
        get
        {
            return parameter.RESOURCE_MIN;
        }
    }
    public float taxRate
    {
        get
        {
            return parameter.TAX_RATE;
        }
    }

    /* population related */
    InfectedTimer CTimer = null; // Current generation counter
    InfectedTimer NTimer = null; // Next generation counter
    public float CR0
    {
        get
        {
            return parameter.R_FACTOR * CTimer.Reproduction;
        }
    }
    public float NR0
    {
        get
        {
            return parameter.R_FACTOR * NTimer.Reproduction;
        }
    }
    public float CDR
    {
        get
        {
            return parameter.D_FACTOR * CTimer.DeathRate;
        }
    }
    public float NDR
    {
        get
        {
            return parameter.D_FACTOR * NTimer.DeathRate;
        }
    }

    public Variable<int> HPCount;
    public Variable<int> CIPCount; // Current generation of infected
    public Variable<int> NIPCount; // Next generation of infected, temporarily not used

    public Variable<int> MaterialCount;

    public void Commit()
    {
        HPCount.Commit();
        CIPCount.Commit();
        NIPCount.Commit();
        MaterialCount.Commit();
    }

    public Block(GameObject mBlockUI)
    {
        GameObject manager = Utility.GetManager();
        Landscape scape = manager.GetComponent<Landscape>();
        VirusModel model = manager.GetComponent<VirusModel>();

        blockUI = mBlockUI;
        Landblock lb = blockUI.GetComponent<Landblock>();
        type = lb.type;
        
        lb.block = this;
        outBlocks = new List<Block>();

        NTimer = scape.NTimer;
        CTimer = scape.CTimer;

        if (model.randomPopulation)
            lb.population = Random.Range(model.randomPopulationMin, model.randomPopulationMax);

        VarIntInit(ref HPCount, lb.population, VarType.HEALTHY_POP);
        VarIntInit(ref CIPCount, 0, VarType.INFECTED_POP_CURR_GEN);
        VarIntInit(ref NIPCount, lb.infected, VarType.INFECTED_POP_NEXT_GEN);
        VarIntInit(ref MaterialCount, lb.material, VarType.MATERIAL);

        /* block type specific */
        if (type == BlockType.FACTORY)
            isWorking = true;

        if (type == BlockType.HOSPITAL)
            CIPCount.priority = 20.0f;

    }

    public void AddOutBlock(Block target)
    {
        outBlocks.Add(target);
        HPCount.AddOutVariable(target.HPCount);
        CIPCount.AddOutVariable(target.CIPCount);
        NIPCount.AddOutVariable(target.NIPCount);
        MaterialCount.AddOutVariable(target.MaterialCount);

    }

    public void VarIntInit(ref Variable<int> variable, int data, VarType type)
    {
        variable = new BroadcastVariableInt(data, type);
        foreach (Block block in outBlocks)
        {
            switch (type)
            {
                case VarType.HEALTHY_POP:
                    {
                        variable.AddOutVariable(block.HPCount);
                        break;
                    }
                case VarType.INFECTED_POP_CURR_GEN:
                    {
                        variable.AddOutVariable(block.CIPCount);
                        break;
                    }
                case VarType.INFECTED_POP_NEXT_GEN:
                    {
                        variable.AddOutVariable(block.NIPCount);
                        break;
                    }
                case VarType.MATERIAL:
                    {
                        variable.AddOutVariable(block.MaterialCount);
                        break;
                    }
                default: break;
            }
        }
    }

    /* 以下方法段是一些与父对象互动的工具方法 */
    public int StopWorking()
    {
        isWorking = false;
        return 0;
    }
    public int StartWorking()
    {
        if (type == BlockType.FACTORY)
        {
            isWorking = true;
            return 0;
        } else
        {
            return 1;
        }
    }

    public int Taxed()
    {
        int taxed = (int)System.Math.Floor(MaterialCount.Data * taxRate);
        MaterialCount.Data -= taxed;
        return taxed;
    }

    int QUARANTINE_PERIOD = -1;
    bool isQuarantined;
    int quarantineCounter;
    public int Quarantined(int period)
    {
        isQuarantined = true;
        QUARANTINE_PERIOD = period;
        quarantineCounter = 0;
        return 0;
    }

    public int Aided()
    {
        HPCount.Data = 0;
        CIPCount.Data = 0;
        NIPCount.Data = 0;
        return 0;
    }

    /* 回合结束时首先执行的块内结算，不涉及与其他块的数据交换 */
    public void EndInBlock(int develop)
    {
        // population counted
        // int develop = CTimer.Tick() + NTimer.Tick();
        if (develop > 0) // pandemics developed
        {
            // shift generation
            HPCount.Data += CIPCount.Data;
            CIPCount.Data = NIPCount.Data;
            NIPCount.Data = 0;
        }
        int inf = (int)(CIPCount.Data * CR0 + NIPCount.Data * NR0);
        if (inf > HPCount.Data) inf = HPCount.Data;
        NIPCount.Data += inf;
        HPCount.Data -= inf;

        int death = Utility.AdaptedRandomNumber(CDR, CIPCount.Data);
        CIPCount.Data -= death;
        death = Utility.AdaptedRandomNumber(NDR, NIPCount.Data);
        NIPCount.Data -= death;

        // material counted
        MaterialCount.Data += (int)System.Math.Floor(MCRate*(HPCount.Data+CIPCount.Data));
        if (MaterialCount.Data < RESOURCE_MIN)
            MaterialCount.Data = RESOURCE_MIN;
    }

    /* 这一阶段涉及的数据计算一般需要依赖于来自其他区块的数据 */
    public void EndRound()
    {
        if (isQuarantined)
        {
            quarantineCounter += 1;
            if (quarantineCounter == QUARANTINE_PERIOD)
            {
                isQuarantined = false;
            }
            return;
        }

        /* old ways of broadcasting
        int len = outBlocks.Count;
        int distr = HPCount.Data / len / 2;
        int cdistr = CIPCount.Data / len / 2;
        int ndistr = NIPCount.Data / len / 2;
        foreach (Block target in outBlocks)
        {
            target.HPCount.DataBuf += distr;
            HPCount.DataBuf -= distr;

            target.CIPCount.DataBuf += cdistr;
            CIPCount.DataBuf -= cdistr;

            target.NIPCount.DataBuf += ndistr;
            NIPCount.DataBuf -= ndistr;
        }
        */

        HPCount.Broadcast(parameter.HP_MOVE_RATIO);
        if (type != BlockType.HOSPITAL)
            CIPCount.Broadcast(parameter.CIP_MOVE_RATIO);
        else
            CIPCount.Broadcast(parameter.CIP_MOVE_RATIO, 2.0f);
        NIPCount.Broadcast(parameter.NIP_MOVE_RATIO);
        MaterialCount.Broadcast(parameter.M_MOVE_RATIO);
    }
}