<template>
  <q-page>
    <div class="header-with-button page-padding">
      <h2 class="q-title">
        {{pageTitle}}
      </h2>
      <q-btn v-if="canPost" no-caps
             @click="$router.push( {name:'CreateMaterial',params:{categoriesNames: categoriesNames}})"
             :label="addButtonLabel" icon="fas fa-plus" color="post"/>
    </div>

    <div v-if="caption" class="page-padding q-mb-lg text-grey-9" style="margin-top: -14px" v-html="caption"></div>


    <PostsList ref="postsList"/>

    <q-pagination class="page-padding q-mt-md" v-if="posts && posts.totalPages > 1" v-model="posts.pageIndex"
                  color="pagination"
                  :max-pages="12" :max="posts.totalPages" ellipses direction-links @input="pageChanges"/>


  </q-page>
</template>

<script>
  import Page from "Page";
  import PostsList from "./PostsList";

  export default {
    name: 'BlogMultiCatPage',
    components: {PostsList},
    mixins: [Page],
    props: {
      categoriesNames: {
        type: String,
        required: true,
      },
      addButtonLabel: {
        type: String,
        required: false,
        default: "Добавить текст"
      },
      pageTitle: {
        type: String,
        required: true
      },
      caption: {
        type: String,
        required: false
      },
      rolesCanAdd: {
        type: Array,
        required: false
      }
    },
    data: function () {
      return {
        posts: null
      }
    },
    watch: {
      'categoriesNames': 'loadData',
      '$route.query.page': 'loadData',
      '$store.state.categories.all': 'loadData',
      '$store.state.auth.user': 'loadData'
    },
    computed: {
      canPost() {
        if (this.rolesCanAdd)
          if (!this.$store.state.auth.roles.some(x => this.rolesCanAdd.some(y => y === x)))
            return false;

        let categories = this.categoriesNames.split(",").map(x => x.trim());
        for (let catName of categories) {
          let cat = this.$store.getters.getCategory(catName);
          if (cat?.canSomeChildrenWriteMaterial) {
            return true;
          }
        }
        return false;
      },
      currentPage() {
        let page = this.$route.query?.page;
        return page ?? 1;
      }
    },
    methods: {
      pageChanges(newPage) {
        if (this.currentPage !== newPage) {
          let req = {path: this.$route.path};
          if (newPage !== 1) {
            req.query = {page: newPage};
          }
          this.$router.push(req);
        }
      },

      async loadData() {

        await this.$store.dispatch("request",
          {
            url: "/Blog/GetPostsFromMultiCategories",
            data: {
              categoriesNames: this.categoriesNames,
              page: this.currentPage
            }
          })
          .then(
            response => {
              this.posts = response.data;
              this.$refs.postsList.posts = response.data;
            }
          ).catch(x => {
            console.log("error", x);
          });
      }
    },
    async created() {
      this.title = this.pageTitle;
      await this.loadData();
    }
  }
</script>


<style lang="stylus" scoped>


</style>
