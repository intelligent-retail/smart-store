<template>
  <div id="monitor" class="section">
    <h1 class="title is-3 has-text-grey">
      リアルタイム在庫モニター - 店舗コード: {{ shop.shopCode }}
    </h1>
    <div class="field">
      <div class="control">
        <a class="button is-primary" v-on:click="init">リセット</a>
      </div>
    </div>
    <div class="box" v-for="(box, key, index) in shop.terminals" :key="index">
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
import { defineComponent, ref, onMounted } from 'vue'
import axios, { AxiosRequestConfig } from 'axios'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

type Item = {
  itemCode: string
  itemName: string | null
  quantity: number
  updated: boolean
}

type Terminal = {
  terminalCode: number
  items: Item[]
}

type Shop = {
  shopCode: string
  terminals: Terminal[]
}

export default defineComponent({
  setup() {
    const baseUrl: string = process.env.VUE_APP_HOST as string
    const companyCode = ref('00100') // TODO: 画面からセットする
    const shop = ref({
      shopCode: '12345', // TODO: 画面からセットする
      terminals: [
        {
          terminalCode: 0,
          items: [
            {
              itemCode: '',
              itemName: null,
              quantity: 0,
              updated: false,
            },
          ],
        },
      ],
    })

    const getAxiosConfig = (): AxiosRequestConfig => {
      const config = {
        headers: {
          'x-functions-key': process.env.VUE_APP_KEY,
        },
      }
      return config
    }

    const sleep = (time: number): Promise<void> => {
      return new Promise(function(resolve) {
        window.setTimeout(resolve, time)
      })
    }

    // データ初期化
    const init = async (): Promise<void> => {
      const route =
        '/v1/company/' +
        companyCode.value +
        '/store/' +
        shop.value.shopCode +
        '/stocks'
      const resp = await axios.post(
        process.env.VUE_APP_HOST + route,
        null,
        getAxiosConfig(),
      )
      shop.value = resp.data.store
      console.log('Initialized!')
    }

    // データ更新
    const update = async (
      terminalCode: number,
      itemCode: string,
    ): Promise<void> => {
      console.log('Called from SignalR! ' + terminalCode + '-' + itemCode)

      // 更新対象のBoxを特定する
      const targetBoxIndex = shop.value.terminals.findIndex(
        (terminal: Terminal) => {
          return terminal.terminalCode === terminalCode
        },
      )

      // 更新対象のItemを特定する
      const targetItemIndex = shop.value.terminals[
        targetBoxIndex
      ].items.findIndex((item: Item) => {
        return item.itemCode === itemCode
      })

      // APIからItemの最新データを取得
      const route =
        '/v1/company/' +
        companyCode.value +
        '/store/' +
        shop.value.shopCode +
        '/terminal/' +
        terminalCode +
        '/item/' +
        itemCode +
        '/stock'
      const resp = await axios.post(
        process.env.VUE_APP_HOST + route,
        null,
        getAxiosConfig(),
      )
      shop.value.terminals[targetBoxIndex].items[targetItemIndex].quantity =
        resp.data.quantity

      // 変更したitemの背景を変化させる
      shop.value.terminals[targetBoxIndex].items[targetItemIndex].updated = true

      await sleep(500)
      shop.value.terminals[targetBoxIndex].items[
        targetItemIndex
      ].updated = false

      console.log('Updated! ' + resp.data.quantity)
    }

    onMounted(
      async (): Promise<void> => {
        console.log('init')

        // 初回のデータを取得する
        await init()

        // SignalRとコネクションを確立する
        const connection = new HubConnectionBuilder()
          .withUrl(baseUrl)
          .configureLogging(LogLevel.Information)
          .build()
        console.log('connecting...')

        // SignalR Serviceへの接続
        connection
          .start()
          .then(() => console.log('connected!'))
          .catch(console.error)

        // SignalRからの呼び出し
        connection.on('update', update)

        // 切断
        connection.onclose(() => console.log('disconnected'))
      },
    )

    return {
      companyCode,
      shop,
      init,
    }
  },
})
</script>
