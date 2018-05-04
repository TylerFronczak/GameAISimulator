//*************************************************************************************************
// Author: https://www.tylerfronczak.com/
//*************************************************************************************************

public interface IDamageable
{
    float Health { get; }

    void ReceiveDamage(float damage);
    void Death();
}
