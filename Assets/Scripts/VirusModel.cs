using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Warning: all parameters will be overwritten at runtime
public class VirusModel : MonoBehaviour
{
    [Header("Virus Timer")]
    /* Virus [R]eproduction calculation:
     * R_in_block = InfectedStage.reproduction  (as GLOBAL R FACTOR, will change with time)
     *            * BlockTypeParameter.R_FACTOR (as LOCAL R FACTOR, block type specific)
     *            + Block.VIRUS_R_FACTOR        (as VIRUS R FACTOR, runtime specific, 
     *                                           change with development of pandemic)
     *                                           
     * At the end of a round, virus will be reproduced in block
     * (that is to say, reproduction in one block is independent 
     *  of other blocks) according to R_in_block. To be specific,
     * infected_population += Random(0, infected_population * R_in_block)
     * Ideally Random() should obey a Poisson distribution or something
     * buf for now it's just uniform. May be modified in the future.
     */
    // InfectedStage(GLOBAL_R_FACTOR, GLOBAL_DR_FACTOR)
    public InfectedStage[] stages = {
         new InfectedStage(0.0f, 0.0f),
         new InfectedStage(1.0f, 0.0f),
         new InfectedStage(1.0f, 0.1f),
         new InfectedStage(1.0f, 0.5f)
     };
    // number of days a stage will last
    public int timerStagePeriod = 3;

    [Header("Virus Effect")]
    // Virus_{k+1} = ( Virus_{k} + VIRUS_SCALING * (NIP + CIP) ) * VIRUS_DECAY
    // VIRUS_R_FACTOR = AMPLITUDE * F( Virus_{k+1} * GRADIENT )
    // when Virus * GRADIENT >= 0.95, the VIRUS_FACTOR  will be nearly AMPLITUDE
    public float virusScaling = 0.1f;
    public float virusDecay = 0.9f;
    public float virusAmplitude = 1.0f;
    public float virusGradient = 0.01f;

    [Header("Block Parameters")]
    // Defined beneath
    public BlockTypeParameter factory = new BlockTypeParameter(BlockType.FACTORY);
    public BlockTypeParameter housing = new BlockTypeParameter(BlockType.HOUSING);
    public BlockTypeParameter hospital = new BlockTypeParameter(BlockType.HOSPITAL);
    public BlockTypeParameter quarantine = new BlockTypeParameter(BlockType.QUARANTINE);

    [Header("Mechanic Options")]
    // set this flag to true if you want to generate random population for all blocks
    // the range of random generation is defined in VirusModel
    public bool randomPopulation = true;
    // random population generation parameter
    public int randomPopulationMin = 400;
    public int randomPopulationMax = 600;

    // set this flag to true if you want to auto-collect and manage materials globally
    public bool autoGlobalTaxing = false;

    // set this flag to true if you want to enable Population Volume mechanism
    public bool enableVolumeConstraint = false;

    // set this flag to true if you want to see all infected
    public bool enableGodView = false;

    [Header("Others")]
    // set this flag to true if you want to use UI version 2
    // UI version 1 for False
    public bool enableUIVer2 = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

/* BlockTypeParameter
 * important parameters that are strongly (or at least potentially) related to block type
 * though some of them may be temporarily the same for all types of blocks
 *  ============= WARNING =============
 *  All parameters coded here will be overwritten by data set in the inspector.
 *  Value here are just for reference.
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
    public float HP_MOVE_RATIO = 0.05f;
    // ratio by which current generation of infected population will move out at most
    public float CIP_MOVE_RATIO = 0.01f;
    // ratio by which next generation of infected population will move out at most
    public float NIP_MOVE_RATIO = 0.05f;
    // ratio by which material will move out at most
    public float M_MOVE_RATIO = 0.5f;
    // ratio by which virus will move out at most
    public float VIRUS_MOVE_RATIO = 0.5f;

    public float CIP_PRIORITY_OFFSET = 0.0f;

    public int POPULATION_VOLUME = 550;

    [System.NonSerialized]
    public int RESOURCE_MIN = 0;

    public BlockTypeParameter(BlockType mType)
    {
        type = mType;
        if (type == BlockType.HOSPITAL)
        {
            R_FACTOR = 0.5f;
            D_FACTOR = 0.0f;
            CONSUME_FACTOR = -0.1f;
            CIP_PRIORITY_OFFSET = 2.0f;
            POPULATION_VOLUME = 900;
        }
        if (type == BlockType.QUARANTINE)
        {
            POPULATION_VOLUME = 1500;
        }
        if (type == BlockType.FACTORY)
        {
            POPULATION_VOLUME = 100;
        }
    }

}

