<template>
  <div id="monitor" class="section">
    <h1 class="title is-3 has-text-grey">
      リアルタイム在庫モニター - 店舗コード: {{ store.storeCode }}
    </h1>
    <div class="field">
      <div class="control">
        <a class="button is-primary" v-on:click="init">リセット</a>
      </div>
    </div>
    <div class="box" v-for="(box, key, index) in store.terminals" :key="index">
      <div class="title is-5 has-text-grey">
        Box番号: {{ box.terminalCode }}
      </div>
      <div class="columns is-multiline">
        <div
          class="column is-3"
          v-for="(item, key, index) in box.items"
          :key="index"
        >
          <div
            class="box item-stock"
            v-if="item.itemName != null"
            :class="{ active: item.updated }"
          >
            <div class="title">
              <span>{{ item.quantity }}</span>
              <figure class="image is-4by3">
                <img :src="item.imageUrls[0]" />
              </figure>
            </div>
            <p class="heading">{{ item.itemName }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.item-stock.active {
  background-color: hsl(48, 100%, 67%);
  transition: 0.5s;
}
</style>

<script lang="ts">
import Vue from "vue";
import axios from "axios";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
  JsonHubProtocol
} from "@aspnet/signalr";

type Item = {
  itemCode: string;
  itemName: string | null;
  quantity: number;
  updated: boolean;
};

type Store = {
  storeCode: string;
  terminals: [
    {
      terminalCode: number;
      items: Item[];
    }
  ];
};

export default Vue.extend({
  data() {
    return {
      baseUrl: process.env.VUE_APP_HOST as string,
      companyCode: "00100" as string, // TODO: 画面からセットする
      store: {
        storeCode: "12345", // TODO: 画面からセットする
        terminals: [
          {
            terminalCode: 0,
            items: [
              {
                itemCode: "",
                itemName: null,
                quantity: 0,
                updated: false
              }
            ]
          }
        ]
      } as Store
    };
  },
  methods: {
    init: async function(): Promise<void> {
      const route =
        "/v1/company/" +
        this.companyCode +
        "/store/" +
        this.store.storeCode +
        "/stocks";
      const resp = await axios.post(
        process.env.VUE_APP_HOST + route,
        null,
        getAxiosConfig()
      );
      this.store = resp.data.store;
      console.log("Initialized!");
    },
    update: async function(
      terminalCode: number,
      itemCode: string
    ): Promise<void> {
      console.log("Called from SignalR! " + terminalCode + "-" + itemCode);

      // 更新対象のBoxを特定する
      const targetBoxIndex = this.store.terminals.findIndex(terminal => {
        return terminal.terminalCode === terminalCode;
      });

      // 更新対象のItemを特定する
      const targetItemIndex = this.store.terminals[
        targetBoxIndex
      ].items.findIndex(item => {
        return item.itemCode === itemCode;
      });

      // APIからItemの最新データを取得
      const route =
        "/v1/company/" +
        this.companyCode +
        "/store/" +
        this.store.storeCode +
        "/terminal/" +
        terminalCode +
        "/item/" +
        itemCode +
        "/stock";
      const resp = await axios.post(
        process.env.VUE_APP_HOST + route,
        null,
        getAxiosConfig()
      );
      this.$set(
        this.store.terminals[targetBoxIndex].items[targetItemIndex],
        "quantity",
        resp.data.quantity
      );

      // 変更したitemの背景を変化させる
      this.$set(
        this.store.terminals[targetBoxIndex].items[targetItemIndex],
        "updated",
        true
      );
      await sleep(500);
      this.$set(
        this.store.terminals[targetBoxIndex].items[targetItemIndex],
        "updated",
        false
      );

      console.log("Updated! " + resp.data.quantity);
    }
  },
  mounted: async function(): Promise<void> {
    // 初回のデータを取得する
    this.init();

    // SignalRとコネクションを確立する
    const connection = new HubConnectionBuilder()
      .withUrl(this.baseUrl)
      .configureLogging(LogLevel.Information)
      .build();
    console.log("connecting...");

    // SignalR Serviceへの接続
    connection
      .start()
      .then(() => console.log("connected!"))
      .catch(console.error);

    // SignalRからの呼び出し
    connection.on("update", this.update);

    // 切断
    connection.onclose(() => console.log("disconnected"));
  }
});

function getAxiosConfig(): Object {
  const config = {
    headers: {
      "x-functions-key": process.env.VUE_APP_KEY
    }
  };
  return config;
}

function sleep(time: number): Promise<void> {
  return new Promise(function(resolve, reject) {
    window.setTimeout(resolve, time);
  });
}
</script>
