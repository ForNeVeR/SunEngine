<template>
  <q-page class="page-padding">
    <h2 class="q-title"> {{$tl("titleStart")}}
      <q-icon name="far fa-user" color="grey-7"/>
      {{userName}}
    </h2>

    <q-editor class="q-mb-md"
              :toolbar="[
          ['bold', 'italic', 'strike', 'underline'],
          ['token', 'hr' ],
          ['quote', 'unordered', 'ordered' ],
          ['undo', 'redo','fullscreen'],
             ]"

              ref="htmlEditor" v-model="text"/>

    <q-btn no-caps icon="fas fa-arrow-circle-right" class="q-mr-sm" @click="send" color="send" :loading="loading"
           :label="$tl('sendBtn')">
      <loader-sent slot="loading"/>
    </q-btn>
    <q-btn no-caps icon="fas fa-times" @click="$router.back()" color="warning" :label="$t('Global.btn.cancel')"/>
  </q-page>
</template>


<script>
  import Page from "Page";
  import LoaderSent from "LoaderSent";

  export default {
    name: "SendPrivateMessage",
    components: {LoaderSent},
    mixins: [Page],
    props: {
      userId: {
        type: String,
        required: true
      },
      userName: {
        type: String,
        required: true
      }
    },
    data: function () {
      return {
        text: "",
        loading: false
      }
    },
    methods: {
      async send() {
        await this.$store.dispatch("request",
          {
            url: "/Profile/SendPrivateMessage",
            data: {
              userId: this.userId,
              text: this.text
            }
          })
          .then( () => {
              const msg = this.$tl("sendSuccessNotify",this.userName);
              this.$q.notify({
                message: msg,
                timeout: 5000,
                color: 'positive',
                position: 'top'
              });
              this.loading = false;
              this.$router.$goBack();
            }
          ).catch(error => {
            this.$errorNotify(error);
            this.loading = false;
          });

      }
    },
    created() {
      this.title = this.$tl("title");
    }

  }
</script>

<style scoped>

</style>
