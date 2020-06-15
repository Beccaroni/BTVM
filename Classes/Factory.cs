namespace BTVM.Classes
{
    public static class Factory
    {
        private static Broadcaster _broadcaster;

        public static Broadcaster Broadcaster()
        {
            return _broadcaster ?? (_broadcaster = new Broadcaster());
        }
    }

}

