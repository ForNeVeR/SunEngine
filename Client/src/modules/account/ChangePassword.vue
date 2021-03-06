<template>
  <q-page class="flex middle page-padding">

    <div class="center-form">
      <q-input ref="passwordOld" v-model="passwordOld" :type="showPasswordOld ? 'text' : 'password'" :label="$tl('passwordOld')" :rules="rules.passwordOld" >
        <template v-slot:prepend>
          <q-icon name="fas fa-key"/>
        </template>
        <template v-slot:append>
          <q-icon
            :name="showPasswordOld ? 'visibility' : 'visibility_off'"
            class="cursor-pointer"
            @click="showPasswordOld = !showPasswordOld"
          />
        </template>
      </q-input>

      <q-input ref="password" v-model="password" :type="showPassword ? 'text' : 'password'" :label="$tl('password')" :rules="rules.password">
        <template v-slot:prepend>
          <q-icon name="fas fa-key"/>
        </template>
        <template v-slot:append>
          <q-icon
            :name="showPassword ? 'visibility' : 'visibility_off'"
            class="cursor-pointer"
            @click="showPassword = !showPassword"
          />
        </template>
      </q-input>

      <q-input ref="password2" v-model="password2" :type="showPassword2 ? 'text' : 'password'" :label="$tl('password2')" :rules="rules.password2">
        <template v-slot:prepend>
          <q-icon name="fas fa-key"/>
        </template>
        <template v-slot:append>
          <q-icon
            :name="showPassword2 ? 'visibility' : 'visibility_off'"
            class="cursor-pointer"
            @click="showPassword2 = !showPassword2"
          />
        </template>
      </q-input>

      <q-btn no-caps class="q-mt-lg" icon="far fa-save" color="send" :label="$tl('changeBtn')" @click="changePassword"
             :loading="submitting">
        <LoaderSent slot="loading"/>
      </q-btn>
    </div>

  </q-page>
</template>

<script>
  import Page from "Page";
  import LoaderSent from "LoaderSent";


  function createRules() {

    const password = [
      value => !!value || this.$tl("validation.password.required"),
      value => value.length >= config.PasswordValidation.MinLength || this.$tl("validation.password.minLength"),
      value => [...new Set(value.split(''))].length >= config.PasswordValidation.MinDifferentChars || this.$tl("validation.password.minDifferentChars"),
    ];

    return {
      passwordOld: [
        value => !!value ||  this.$tl("validation.passwordOld.required"),
      ],
      password: password,
      password2: [...password,
        value => this.password === this.password2 || this.$tl("validation.password2.equals")]
    }
  }


  export default {
    name: "ChangePassword",
    components: {LoaderSent},
    mixins: [Page],
    data: function () {
      return {
        passwordOld: "",
        password: "",
        password2: "",
        submitting: false,
        showPasswordOld: false,
        showPassword: false,
        showPassword2: false,
      }
    },
    rules: null,
    methods: {
      async changePassword() {
        this.$refs.passwordOld.validate();
        this.$refs.password.validate();
        this.$refs.password2.validate();

        if (this.$refs.passwordOld.hasError || this.$refs.password.hasError || this.$refs.password2.hasError) {
          return;
        }

        this.submitting = true;

        await this.$store.dispatch('request', {
          url: '/Account/ChangePassword',
          data: {
            passwordOld: this.passwordOld,
            passwordNew: this.password
          }
        }).then(response => {
          const msg = this.$tl("successNotify");
          this.$q.notify({
            message: msg,
            timeout: 2800,
            color: 'positive',
            icon: 'fas fa-check-circle',
            position: 'top'
          });
          this.submitting = false;
          this.$router.back();
        }).catch(error => {
          this.$errorNotify(error.response.data);
          this.submitting = false;
        });
      }
    },
    created() {
      this.title = this.$tl("title");
      this.rules = createRules.call(this);
    }
  }
</script>

<style lang="stylus" scoped>


</style>
