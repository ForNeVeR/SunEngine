<template>
  <q-page class="flex middle page-padding">
    <q-banner rounded class="bg-negative text-white">
      <template v-slot:avatar>
        <q-icon name="fas fa-exclamation-circle" size="2em"/>
      </template>
      {{$tl("message")}}
    </q-banner>
  </q-page>
</template>

<script>
  import Page from 'Page'

  export default {
    name: "ResetPasswordFailed",
    mixins: [Page],
    created() {
      this.title = this.$tl("title");
    }
  }
</script>

<style scoped>

</style>
