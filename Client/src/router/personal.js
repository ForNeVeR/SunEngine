import SettingsPanel from 'personal/SettingsPanel';
import SettingsPage from 'personal/SettingsPage';
import LoadPhoto from 'personal/LoadPhoto';
import EditInformation from 'personal/EditInformation';
import ChangeLink from 'personal/ChangeLink.vue';
import ChangeName from 'personal/ChangeName.vue';
import MyBanList from 'personal/MyBanList.vue';
import Profile from 'profile/Profile.vue';

import {store} from 'store';


const routes = [
  {
    name: 'ChangeLink',
    path: '/personal/ChangeLink'.toLowerCase(),
    components: {
      default: ChangeLink,
      navigation: SettingsPanel
    }
  },
  {
    name: 'ChangeName',
    path: '/personal/ChangeName'.toLowerCase(),
    components: {
      default: ChangeName,
      navigation: SettingsPanel
    }
  },
  {
    name: 'Personal',
    path: '/personal',
    components: {
      default: SettingsPage,
      navigation: null
    }
  },
  {
    name: 'LoadPhoto',
    path: '/personal/LoadPhoto'.toLowerCase(),
    components: {
      default: LoadPhoto,
      navigation: SettingsPanel
    }
  },
  {
    name: 'EditInformation',
    path: '/personal/EditInformation'.toLowerCase(),
    components: {
      default: EditInformation,
      navigation: SettingsPanel
    }
  },
  {
    name: 'MyBanList',
    path: '/personal/MyBanList'.toLowerCase(),
    components: {
      default: MyBanList,
      navigation: SettingsPanel
    }
  },
  {
    name: 'ProfileInSettings',
    path: '/personal/Profile'.toLowerCase(),
    components: {
      default: Profile,
      navigation: SettingsPanel
    },
    props: {
      default: () => {
        return {link: store.state.auth.userInfo?.link}
      }
    }
  },


];


for (let route of routes) {
  if (!route.meta) {
    route.meta = {
      roles: ["Registered"]
    };
  }
}

export default routes
