using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LolWikiApp
{
    public class VideoTypeListItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalDataTemplate { get; set; }
        public DataTemplate UpdatedDataTemplate { get; set; }

        public override DataTemplate SelectDataTemplate(object item, DependencyObject container)
        {
            var news = item as VideoTypeListInfo;

            if (news != null)
            {
                var span = DateTime.Now - news.Date;
                if (span.Days < 1 )
                {
                    return UpdatedDataTemplate;
                }
                return NormalDataTemplate;
            }

            return base.SelectDataTemplate(item, container);
        }
    }
}
