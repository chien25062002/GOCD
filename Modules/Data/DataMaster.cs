using UnityEngine;

namespace GOCD.Framework.Data
{
    public class DataMaster : GOCDDataBlock<DataMaster>
    {
        [SerializeField] GOCDValue<bool> _adsRewardSkip;
        [SerializeField] GOCDValue<bool> _adsInterSkip;
        [SerializeField] GOCDValue<bool> _adsBannerSkip;
        [SerializeField] GOCDValue<bool> _uiHidden;
        
        public static GOCDValue<bool> AdsRewardSkip => instance._adsRewardSkip;
        public static GOCDValue<bool> AdsInterSkip => instance._adsInterSkip;
        public static GOCDValue<bool> AdsBannerSkip => instance._adsBannerSkip;
        public static GOCDValue<bool> UiHidden => instance._uiHidden;

        protected override void Init()
        {
            base.Init();
            
            _uiHidden ??= new GOCDValue<bool>(false);
            _adsRewardSkip ??= new GOCDValue<bool>(false);
            _adsInterSkip ??= new GOCDValue<bool>(false);
            _adsBannerSkip ??= new GOCDValue<bool>(false);
        }
    }
}
