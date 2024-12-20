using System.Collections.Generic;

namespace ET
{
    public partial class SystemMenuCategory
    {
        public Dictionary<int, List<SystemMenu>> ClassifyDict { get; } = new Dictionary<int, List<SystemMenu>>();

        public List<SystemMenu> GetList(int classify)
        {
            this.ClassifyDict.TryGetValue(classify, out List<SystemMenu> list);
            return list;
        }

        private void AddClassifyMenu(SystemMenu config)
        {
            var classify = config.Classify;
            if (!this.ClassifyDict.TryGetValue(classify, out var list))
            {
                list = new List<SystemMenu>();
                this.ClassifyDict.Add(classify, list);
            }

            list.Add(config);
        }

        public void AddDynamicMenu(SystemMenu config)
        {
            this.dict.Add(config.Id, config);
            this.AddClassifyMenu(config);
        }

        public void RemoveDynamicMenu(int id)
        {
            if (!this.dict.Remove(id, out SystemMenu config))
            {
                return;
            }

            if (this.ClassifyDict.TryGetValue(config.Classify, out var list))
            {
                list.Remove(config);
            }
        }

        public void RemoveDynamicClassify(int classify)
        {
            if (!this.ClassifyDict.Remove(classify, out List<SystemMenu> list))
            {
                return;
            }

            foreach (SystemMenu menu in list)
            {
                this.dict.Remove(menu.Id);
            }
        }

        public override void EndInit()
        {
            foreach (SystemMenu config in this.dict.Values)
            {
                this.AddClassifyMenu(config);
            }
        }
    }

    public partial class SystemMenu
    {
        public int GroupId { get; set; }

        public override void EndInit()
        {
        }
    }
}