using Terraria;

namespace SariaMod
{
    internal static class VisualDustLimiter
    {
        private static ulong lastCapacityCheckTick = ulong.MaxValue;
        private static int activeDustCountWithReservations;

        /// <summary>
        /// Reserves one visual dust slot while keeping all participating effects below
        /// half of Terraria's dust pool. Reservations are shared for the current update.
        /// </summary>
        public static bool TryReserveHalfCapacitySlot()
        {
            return TryReserveHalfCapacitySlots(1);
        }

        /// <summary>
        /// Atomically reserves several dust slots for an effect that must spawn as a group.
        /// </summary>
        public static bool TryReserveHalfCapacitySlots(int slots)
        {
            if (slots <= 0)
            {
                return true;
            }

            if (Main.dedServ)
            {
                return false;
            }

            int halfDustLimit = Main.maxDust / 2;
            ulong currentTick = Main.GameUpdateCount;
            if (lastCapacityCheckTick != currentTick)
            {
                lastCapacityCheckTick = currentTick;
                activeDustCountWithReservations = 0;

                for (int i = 0; i < Main.maxDust; i++)
                {
                    if (!Main.dust[i].active)
                    {
                        continue;
                    }

                    activeDustCountWithReservations++;
                    if (activeDustCountWithReservations >= halfDustLimit)
                    {
                        break;
                    }
                }
            }

            if (activeDustCountWithReservations + slots > halfDustLimit)
            {
                return false;
            }

            activeDustCountWithReservations += slots;
            return true;
        }
    }
}
