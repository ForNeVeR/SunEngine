<template>
  <div v-if="material">
    <div v-html="material.text"></div>
  </div>
</template>

<script>
  export default {
    name: "MaterialInline",
    props: {
      name: {
        type: String,
        required: true
      }
    },
    data: function () {
      return {
        material: null
      }
    },
    methods: {
      async loadMaterial() {
        await this.$store.dispatch("request",
          {
            url: "/Materials/Get",
            data: {
              idOrName: this.name
            }
          }).then(
          response => {
            this.material = response.data;
          }
        ).catch(x => {
          console.log("error", x);
        });
      },
    },
    async created() {
      await this.loadMaterial();
    }
  }
</script>

<style scoped>

</style>
