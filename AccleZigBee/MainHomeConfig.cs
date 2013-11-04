
namespace AccleZigBee
{
    public partial class MainHome
    {
        public void setFilter(bool value)
        {
            filter = value;
        }
        public bool getFilter()
        {
            return filter;
        }
        public bool getModify()
        {
            return modify;
        }
        public void setModify(bool value)
        {
            modify = value;
        }
        public bool getLog()
        {
            return logData;
        }
        public void setLog(bool value)
        {
            logData = value;
        }
    }
}
