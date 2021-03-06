<template>
  <q-page class="flex flex-center">
    <div class="center-form">
      <div class="text-grey-7 q-mb-lg" style="text-align: justify">
        {{$tl('linkValidationInfo')}}
      </div>
      <q-input ref="link" v-model="link" :label="$tl('link')" @keyup="checkLinkInDb" :rules="linkRules">
        <template v-slot:prepend>
          <q-icon name="fas fa-link"/>
        </template>
      </q-input>
      <q-btn no-caps class="q-mt-lg" icon="far fa-save" :label="$tl('saveBtn')" color="send" @click="save"
             :loading="submitting">
        <LoaderSent slot="loading"/>
      </q-btn>
    </div>
  </q-page>
</template>

<script>
  import Page from "Page";
  import LoaderSent from "LoaderSent";
  import {store} from "store";

  function allowMyIdOrEmpty(id) {
    return !id || store.state.auth.user.id == id;
  }

  function createLinkRules() {
    return [
      value => (value.length >= 3 || allowMyIdOrEmpty.call(this, value)) || this.$tl("validation.minLength"),  // minLength or myId
      value => /^[a-zA-Z0-9-]*$/.test(value) || this.$tl("validation.allowedChars"), // allowed chars
      value => (/[a-zA-Z]/.test(value) || allowMyIdOrEmpty.call(this, value)) || this.$tl("validation.numberNotAllow"), // need char or myId
      value => !this.linkInDb || this.$tl("validation.linkInDb"), // link in db
    ];
  }

  export default {
    name: "ChangeLink",
    mixins: [Page],
    components: {LoaderSent},
    data: function () {
      return {
        link: this.$store.state.auth.userInfo.link,
        linkInDb: false,
        submitting: false
      }
    },
    linkRules: null,
    methods: {
      checkLinkInDb() {
        clearTimeout(this.timeout);
        if (this.link.toLowerCase() === this.$store.state.auth.userInfo.link.toLowerCase())
          return;
        this.timeout = setTimeout(this.checkLinkInDbDo, 500);
      },
      checkLinkInDbDo() {
        this.$store.dispatch("request",
          {
            url: "/Personal/CheckLinkInDb",
            data: {
              link: this.link
            }
          }).then(response => {
          this.linkInDb = response.data.yes;
          this.$refs.link.validate();
        })
      },

      async save() {

        this.$refs.link.validate();

        if (this.$refs.link.hasError) {
          return;
        }


        this.submitting = true;

        await this.$store.dispatch("request",
          {
            url: "/Personal/SetMyLink",
            data: {
              link: this.link
            }
          }).then(response => {
          this.$store.commit('setUserInfo', response.data);
          const msg = this.$tl("linkEditedMessage");
          this.$q.notify({
            message: msg,
            timeout: 2800,
            color: 'positive',
            icon: 'fas fa-check-circle',
            position: 'top'
          });
          this.$router.push({name: 'Personal'});

        }).catch(error => {
          this.$errorNotify(error);
          this.submitting = false;
        });
      }
    },
    beforeDestroy() {
      clearTimeout(this.timeout);
    },
    async created() {
      this.title = this.$tl("title");

      this.linkRules = createLinkRules.call(this);
    }
  }
</script>

<style lang="stylus" scoped>


</style>
