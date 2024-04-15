public class AttackInfo
{
    public InPlaySlot target;
    public FightClub fightManager;

    public AttackInfo(InPlaySlot target, FightClub fightManager)
    {
        this.target = target;
        this.fightManager = fightManager;
    }
}
