using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum VarType
{
    HEALTHY_POP = 1,
    INFECTED_POP_CURR_GEN = 2,
    INFECTED_POP_NEXT_GEN = 3,
    MATERIAL = 4,
    VIRUS_COUNT = 5,
    NONE = 0
}

public enum BlockType
{
    ERROR = 0,
    FACTORY = 1,
    HOUSING = 2,
    HOSPITAL = 3,
    QUARANTINE = 4
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

public class Block
{
    public BlockTypeParameter parameter;
    VirusModel model;
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

    InfectedTimer CTimer = null; // Current generation Timer
    InfectedTimer NTimer = null; // Next generation Timer
    // Local R0(Reproduction) and DR(Death Rate) factor definition.
    // Used in EndInBlock()
    // R0 for CIP
    public float CR0
    {
        get
        {
            return VIRUS_R_FACTOR + parameter.R_FACTOR * CTimer.Reproduction;
        }
    }
    // R0 for NIP
    public float NR0
    {
        get
        {
            return VIRUS_R_FACTOR + parameter.R_FACTOR * NTimer.Reproduction;
        }
    }
    // DR for CIP
    public float CDR
    {
        get
        {
            return parameter.D_FACTOR * CTimer.DeathRate;
        }
    }
    // DR for NIP
    public float NDR
    {
        get
        {
            return parameter.D_FACTOR * NTimer.DeathRate;
        }
    }

    public Variable<int> HPCount;       // Healthy Population
    public Variable<int> CIPCount;      // Current generation of Infected Population
    public Variable<int> NIPCount;      // Next generation of Infected Population

    // Virus_{k+1} = ( Virus_{k} + VIRUS_SCALING * (NIP + CIP) ) * VIRUS_DECAY
    // VIRUS_R_FACTOR = AMPLITUDE * F( Virus * GRADIENT )
    // when Virus * GRADIENT >= 0.95, the VIRUS_FACTOR  will be nearly AMPLITUDE
    // all parameters above are defined in VirusModel
    public float VIRUS_R_FACTOR
    {
        get
        {
            return (float)(model.virusAmplitude * System.Math.Atan(VirusCount.Data * model.virusGradient * 12 ));
        }
    }
    public Variable<int> VirusCount;    // Simulation of spreading trace of virus

    public Variable<int> MaterialCount; // Material count

    public void Commit()
    {
        HPCount.Commit();
        CIPCount.Commit();
        NIPCount.Commit();
        VirusCount.Commit();
        MaterialCount.Commit();
    }

    public Block(GameObject mBlockUI)
    {
        GameObject manager = Utility.GetManager();
        Landscape scape = manager.GetComponent<Landscape>();
        model = manager.GetComponent<VirusModel>();

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
        VarIntInit(ref VirusCount, lb.virus, VarType.VIRUS_COUNT);

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
                case VarType.VIRUS_COUNT:
                    {
                        variable.AddOutVariable(block.VirusCount);
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
    public int TaxAll()
    {
        int taxed = MaterialCount.Data;
        MaterialCount.Data = 0;
        return taxed;
    }

    public int GetInfectedPopulation()
    {
        return CIPCount.Data + NIPCount.Data;
    }

    public int GetTotalPopulation()
    {
        return HPCount.Data + CIPCount.Data + NIPCount.Data;
    }

    public int GetConfirmedInfectedPopulation()
    {
        return CIPCount.Data;
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
        HPCount.Data = HPCount.Data +  NIPCount.Data; //原因在于潜伏期人会非常多,影响视觉感受
        CIPCount.Data = 0;
        NIPCount.Data = 0;
        return 0;
    }

    /* 回合结束时首先执行的块内结算，不涉及与其他块的数据交换 */
    public void EndInBlock(int develop)
    {
        /* virus update */
        VirusCount.Data = (int)System.Math.Ceiling((VirusCount.Data + model.virusScaling * (CIPCount.Data + NIPCount.Data)) * model.virusDecay);
        
        /* population update */
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

        /* material update */
        MaterialCount.Data += (int)System.Math.Floor(MCRate*(HPCount.Data+CIPCount.Data));
        if (!(model.autoGlobalTaxing) && MaterialCount.Data < RESOURCE_MIN)
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

        if (model.enableVolumeConstraint)
        {
            int count = GetTotalPopulation();
            CIPCount.priority = NIPCount.priority = HPCount.priority = parameter.POPULATION_VOLUME - count;
        }

        VirusCount.Broadcast(parameter.VIRUS_MOVE_RATIO);

        HPCount.Broadcast(parameter.HP_MOVE_RATIO);
        CIPCount.Broadcast(parameter.CIP_MOVE_RATIO, parameter.CIP_PRIORITY_OFFSET);
        NIPCount.Broadcast(parameter.NIP_MOVE_RATIO);

        MaterialCount.Broadcast(parameter.M_MOVE_RATIO);
    }
}